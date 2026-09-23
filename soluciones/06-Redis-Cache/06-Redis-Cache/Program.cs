using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using _06_Redis_Cache.Config;
using _06_Redis_Cache.Services;

// ============================================================
// Ejemplo 05: Redis con Cache-Aside (StackExchange.Redis)
// ============================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Ejemplo 05: Redis + Cache-Aside Pattern ===\n");

// Configurar configuración desde appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var redisConfig = new RedisConfig();
configuration.GetSection("RedisSettings").Bind(redisConfig);

// Registrar Inyección de Dependencias
var services = new ServiceCollection();
services.AddSingleton(redisConfig);

// Conectar a Redis usando configuración desde appsettings.json
var redisConnection = await ConnectionMultiplexer.ConnectAsync(redisConfig.ConnectionString);
services.AddSingleton<IConnectionMultiplexer>(redisConnection);
services.AddSingleton<ICacheService, RedisCacheService>();
services.AddSingleton<IProductoService, ProductoService>();

var provider = services.BuildServiceProvider();
var productoService = provider.GetRequiredService<IProductoService>();

// ============================================================
// ⚠️ LIMPIEZA AUTOMÁTICA — SOLO PARA USO EN CLASE
// Controlada por "DropData": true en appsettings.json.
// NUNCA pongas DropData en true en producción.
// ============================================================
if (redisConfig.DropData)
{
    var database = redisConnection.GetDatabase();
    var server = redisConnection.GetServer(redisConnection.GetEndPoints().First());
    var keys = server.Keys(pattern: "*").ToList();
    foreach (var key in keys)
    {
        database.KeyDelete(key);
    }
    Console.WriteLine($"[LIMPIEZA] Caché Redis vaciada ({keys.Count} claves eliminadas, DropData=true)\n");
}

// ============================================================
// Demo: Cache-Aside con productos
// ============================================================
Console.WriteLine("--- PRIMERA CONSULTA (Cache Miss) ---");
Console.WriteLine("Buscando producto ID=1...");
var producto1 = await productoService.GetByIdAsync(1);
Console.WriteLine($"  Resultado: {producto1?.Nombre} - {producto1?.Precio:C}\n");

Console.WriteLine("--- SEGUNDA CONSULTA (Cache Hit) ---");
Console.WriteLine("Buscando producto ID=1 de nuevo...");
var producto1Cache = await productoService.GetByIdAsync(1);
Console.WriteLine($"  Resultado: {producto1Cache?.Nombre} - {producto1Cache?.Precio:C}\n");

Console.WriteLine("--- OBTENER TODOS LOS PRODUCTOS ---");
var todos = await productoService.GetAllAsync();
Console.WriteLine($"  Total: {todos.Count} productos");
foreach (var p in todos)
{
    Console.WriteLine($"  [{p.Id}] {p.Nombre} - {p.Precio:C} ({p.Categoria})");
}

Console.WriteLine("\n--- CONSULTA DESPUÉS DE INVALIDACIÓN ---");
Console.WriteLine("Actualizando precio del producto 1 a 1199.99€...");
await productoService.UpdateAsync(1, 1199.99m);

Console.WriteLine("\nBuscando producto ID=1 tras actualización...");
var producto1Actualizado = await productoService.GetByIdAsync(1);
Console.WriteLine($"  Resultado: {producto1Actualizado?.Nombre} - {producto1Actualizado?.Precio:C}");

Console.WriteLine("\n=== Fin del ejemplo ===");
