using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _05_MongoDB_Driver_EFCore.Config;
using _05_MongoDB_Driver_EFCore.Entity;
using _05_MongoDB_Driver_EFCore.Models;
using _05_MongoDB_Driver_EFCore.Repositories.Base;
using _05_MongoDB_Driver_EFCore.Repositories.MongoDriver;
using _05_MongoDB_Driver_EFCore.Repositories.MongoEfCore;

// ============================================================
// Ejemplo 04: MongoDB con Driver Nativo y EF Core
// ============================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Ejemplo 04: MongoDB con Driver y EF Core ===\n");

// Configurar configuración desde appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var mongoConfig = new MongoConfig();
configuration.GetSection("MongoSettings").Bind(mongoConfig);

// Registrar Inyección de Dependencias
var services = new ServiceCollection();
services.AddSingleton(mongoConfig);

// Registrar repositorio (Driver nativo por defecto, EF Core con argumento)
var useEfCore = args.Length > 0 && args[0].ToLower() == "efcore";

if (useEfCore)
{
    services.AddDbContext<MongoDbContext>(options =>
        options.UseMongoDB(mongoConfig.ConnectionString, mongoConfig.DatabaseName));
    services.AddScoped<IProductoRepository, ProductoMongoEfRepository>();
}
else
{
    services.AddSingleton<IProductoRepository>(
        new ProductoMongoDriverRepository(mongoConfig.ConnectionString, mongoConfig.DatabaseName));
}

var provider = services.BuildServiceProvider();
var repository = provider.GetRequiredService<IProductoRepository>();

var repoType = useEfCore ? "EF Core" : "Driver Nativo";
Console.WriteLine($"Usando repositorio: {repoType}\n");

// ============================================================
// ⚠️ LIMPIEZA AUTOMÁTICA — SOLO PARA USO EN CLASE
// Controlada por "DropData": true en appsettings.json.
// NUNCA pongas DropData en true en producción.
// ============================================================
if (mongoConfig.DropData)
{
    var client = new MongoDB.Driver.MongoClient(mongoConfig.ConnectionString);
    var database = client.GetDatabase(mongoConfig.DatabaseName);
    await database.DropCollectionAsync("productos");
    Console.WriteLine("  [LIMPIEZA] Colección 'productos' vaciada (DropData=true)\n");
}

// ============================================================
// CRUD de Productos
// ============================================================

// CREATE
Console.WriteLine("--- CREATE ---");
var nuevoProducto = new Producto
{
    Nombre = "Webcam HD",
    Precio = 59.99m,
    Categoria = "Periféricos"
};
var creado = await repository.CreateAsync(nuevoProducto);
Console.WriteLine($"  Creado: [{creado.Id}] {creado.Nombre} - {creado.Precio:C}");

// READ ALL
Console.WriteLine("\n--- READ ALL ---");
var productos = await repository.GetAllAsync();
foreach (var p in productos)
{
    Console.WriteLine($"  [{p.Id[..8]}...] {p.Nombre} - {p.Precio:C} ({p.Categoria})");
}

// READ BY ID
Console.WriteLine("\n--- READ BY ID ---");
var encontrado = await repository.GetByIdAsync(creado.Id);
if (encontrado is not null)
{
    Console.WriteLine($"  Encontrado: [{encontrado.Id[..8]}...] {encontrado.Nombre}");
}

// UPDATE
Console.WriteLine("\n--- UPDATE ---");
var actualizado = new Producto
{
    Id = creado.Id,
    Nombre = creado.Nombre,
    Precio = 54.99m,
    Categoria = creado.Categoria,
    CreatedAt = creado.CreatedAt,
    UpdatedAt = DateTime.UtcNow
};
var resultado = await repository.UpdateAsync(actualizado);
Console.WriteLine(resultado is not null
    ? $"  Actualizado: [{resultado.Id[..8]}...] {resultado.Nombre} - {resultado.Precio:C}"
    : "  No encontrado");

// DELETE
Console.WriteLine("\n--- DELETE ---");
var eliminado = await repository.DeleteAsync(creado.Id);
Console.WriteLine(eliminado
    ? $"  Eliminado producto con ID {creado.Id[..8]}..."
    : $"  No se pudo eliminar producto con ID {creado.Id[..8]}...");

// Verificar
var productosFinales = await repository.GetAllAsync();
Console.WriteLine($"\nTotal productos después de DELETE: {productosFinales.Count()}");

Console.WriteLine("\n=== Fin del ejemplo ===");
