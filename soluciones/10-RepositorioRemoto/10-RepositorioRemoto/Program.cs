using System.Text;
using _10_RepositorioRemoto.Config;
using _10_RepositorioRemoto.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.OutputEncoding = Encoding.UTF8;

// ============================================================
// Ejemplo 10: Repositorio Remoto - Servicio con 3 niveles
// ============================================================
Console.WriteLine("=== Ejemplo 10: Repositorio Remoto ===");
Console.WriteLine("Arquitectura: Caché → BD Local → API Remota\n");

// ============================================================
// El entorno decide qué appsettings.{Entorno}.json se carga.
//   DOTNET_ENVIRONMENT=Development → SQLite + MemoryCache
//   DOTNET_ENVIRONMENT=Production  → PostgreSQL + Redis
// Si no se indica nada, asumimos Development para que `dotnet run`
// funcione en clase sin infraestructura externa.
// En docker compose SIEMPRE se fuerza Production.
// ============================================================
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environments.Development;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    EnvironmentName = environment,
    // La base de config es la carpeta del binario: así los appsettings*.json
    // se encuentran tanto con `dotnet run` como dentro del contenedor.
    ContentRootPath = AppContext.BaseDirectory
});

// ============================================================
// Orden de sobrescritura (los últimos ganan):
//   appsettings.json → appsettings.{Entorno}.json → variables de entorno → args
// Por eso en Docker basta con declarar InfraSettings__Cache=Redis
// ============================================================
builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("ApiSettings"));

DependenciesProvider.ConfigureServices(builder.Services, builder.Configuration);

var host = builder.Build();

// Leer configuración para DropData y mostrar la infraestructura activa
var appConfig = new AppConfig();
builder.Configuration.GetSection("ApiSettings").Bind(appConfig);

var infra = new InfraSettings();
builder.Configuration.GetSection("InfraSettings").Bind(infra);

Console.WriteLine($"  Entorno : {environment}");
Console.WriteLine($"  BD      : {infra.Database}");
Console.WriteLine($"  Caché   : {infra.Cache}\n");

DependenciesProvider.Initialize(host.Services, appConfig);

// ============================================================
// 1) Demostración: se ejecuta dentro de un ámbito propio.
//    App es scoped (consume IUserService → AppDbContext), así que
//    NO se resuelve desde la raíz del contenedor.
//
//    El ámbito lo delimita la función local de abajo: 'using var'
//    se dispone al salir de ella, antes de arrancar el host.
// ============================================================
await EjecutarAppEnAmbitoPropioAsync(host.Services);

// ============================================================
// 2) Arrancamos el host: el proceso queda vivo y el
//    UserSyncBackgroundService sigue sincronizando cada N segundos.
//    Se detiene con Ctrl+C (local) o con SIGTERM (docker stop).
// ============================================================
await host.StartAsync();
Console.WriteLine("\n  ⏳ Host arrancado. Pulsa Ctrl+C para detener...\n");

await host.WaitForShutdownAsync();

// ============================================================
// Función local (top-level statements): ejecuta App en su propio
// ámbito scoped. 'using var' libera el scope al salir de aquí,
// igual que haría un bloque 'using (var ...)', pero sin anidamiento.
// ============================================================
static async Task EjecutarAppEnAmbitoPropioAsync(IServiceProvider servicios)
{
    using var scope = servicios.CreateScope();
    var app = scope.ServiceProvider.GetRequiredService<App>();
    await app.RunAsync();
}
