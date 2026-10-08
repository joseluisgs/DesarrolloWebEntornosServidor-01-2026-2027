using _10_RepositorioRemoto.Api;
using _10_RepositorioRemoto.Cache;
using _10_RepositorioRemoto.Config;
using _10_RepositorioRemoto.Entity;
using _10_RepositorioRemoto.Notifications;
using _10_RepositorioRemoto.Repositories;
using _10_RepositorioRemoto.Services;
using _10_RepositorioRemoto.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using StackExchange.Redis;

namespace _10_RepositorioRemoto.Infrastructure;

/// <summary>
/// Proveedor de dependencias para configurar la Inyección de Dependencias.
/// La infraestructura (BD y caché) es <b>opcional</b>: se elige según la sección
/// <c>InfraSettings</c> del appsettings del entorno activo.
/// </summary>
public static class DependenciesProvider
{
    private static bool _initialized;

    /// <summary>
    /// Registra todos los servicios en el contenedor, resolviendo primero
    /// qué infraestructura toca para el entorno en ejecución.
    /// </summary>
    /// <param name="services">Colección de servicios de DI.</param>
    /// <param name="configuration">Configuración ya cargada (appsettings + entorno).</param>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ============================================================
        // InfraSettings decide las implementaciones concretas:
        //   Development → Sqlite + Memory
        //   Production  → PostgreSql + Redis
        // ============================================================
        var infra = new InfraSettings();
        configuration.GetSection("InfraSettings").Bind(infra);
        services.AddSingleton(infra);

        RegisterDatabase(services, infra);
        RegisterCache(services, infra);

        // ============================================================
        // Componentes comunes: no dependen del entorno
        // ============================================================
        var apiBaseUrl = configuration[$"{nameof(AppConfig)}:ApiBaseUrl"]
            ?? configuration["ApiSettings:ApiBaseUrl"]
            ?? "https://jsonplaceholder.typicode.com";

        services.AddRefitClient<IJsonPlaceholderApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<INotificationService, ConsoleNotificationService>();
        services.AddHostedService<UserSyncBackgroundService>();

        // App consume IUserService (scoped): por eso se registra como scoped
        // y se resuelve desde un ámbito en Program.cs, no desde la raíz.
        services.AddScoped<App>();
    }

    /// <summary>
    /// Registra el proveedor de base de datos indicado en <see cref="InfraSettings.Database"/>.
    /// </summary>
    private static void RegisterDatabase(IServiceCollection services, InfraSettings infra)
    {
        var connectionString = infra.Database switch
        {
            DatabaseProviders.PostgreSql => infra.ConnectionStrings.PostgreSql,
            DatabaseProviders.Sqlite => infra.ConnectionStrings.Sqlite,
            _ => throw new InvalidOperationException(
                $"Proveedor de base de datos desconocido: '{infra.Database}'. " +
                $"Usa '{DatabaseProviders.Sqlite}' o '{DatabaseProviders.PostgreSql}'.")
        };

        services.AddDbContext<AppDbContext>(options =>
        {
            if (infra.Database == DatabaseProviders.PostgreSql)
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });
    }

    /// <summary>
    /// Registra el proveedor de caché indicado en <see cref="InfraSettings.Cache"/>.
    /// </summary>
    private static void RegisterCache(IServiceCollection services, InfraSettings infra)
    {
        switch (infra.Cache)
        {
            case CacheProviders.Redis:
                // La conexión se abre de forma perezosa (lazy): solo cuando se resuelve
                // ICacheService por primera vez, no al arrancar el proceso.
                services.AddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(infra.ConnectionStrings.Redis));
                services.AddSingleton<ICacheService, RedisCacheService>();
                break;

            case CacheProviders.Memory:
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, MemoryCacheService>();
                break;

            default:
                throw new InvalidOperationException(
                    $"Proveedor de caché desconocido: '{infra.Cache}'. " +
                    $"Usa '{CacheProviders.Memory}' o '{CacheProviders.Redis}'.");
        }
    }

    /// <summary>
    /// Inicializa la BD y aplica la limpieza inicial si DropData está activo.
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider, AppConfig config)
    {
        if (_initialized) return;

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        // ============================================================
        // ⚠️ LIMPIEZA AUTOMÁTICA — SOLO PARA USO EN CLASE
        // Controlada por "DropData": true en appsettings.json.
        // NUNCA pongas DropData en true en producción.
        // ============================================================
        if (config.DropData)
        {
            context.Users.ExecuteDelete();
            Console.WriteLine("[LIMPIEZA] Tabla Users vaciada (DropData=true)\n");
        }

        _initialized = true;
    }
}
