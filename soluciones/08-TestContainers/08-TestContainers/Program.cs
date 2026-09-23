using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using _08_TestContainers.Config;
using _08_TestContainers.Entity;
using _08_TestContainers.Models;
using _08_TestContainers.Repositories;

// ============================================================
// Ejemplo 07: Entity Framework Core con PostgreSQL
// ============================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Ejemplo 07: EF Core + PostgreSQL ===\n");

// Configurar configuración desde appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var dbConfig = new DatabaseConfig();
configuration.GetSection("DatabaseSettings").Bind(dbConfig);

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(dbConfig.ConnectionString);

using var context = new AppDbContext(optionsBuilder.Options);

// Crear tablas si no existen
await context.Database.EnsureCreatedAsync();

// ============================================================
// ⚠️ LIMPIEZA AUTOMÁTICA — SOLO PARA USO EN CLASE
// Controlada por "DropData": true en appsettings.json.
// NUNCA pongas DropData en true en producción.
// ============================================================
if (dbConfig.DropData)
{
    await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Productos RESTART IDENTITY");
    Console.WriteLine("[LIMPIEZA] Tabla Productos vaciada (DropData=true)\n");
}

var repository = new ProductoRepository(context);

// CREATE
Console.WriteLine("--- CREATE ---");
var nuevoProducto = await repository.CreateAsync(
    new Producto(0, "Portátil HP Pavilion", 899.99m, "Informática"));
Console.WriteLine($"  Creado: [{nuevoProducto.Id}] {nuevoProducto.Nombre} - {nuevoProducto.Precio:C}");

// READ ALL
Console.WriteLine("\n--- READ ALL ---");
var productos = await repository.GetAllAsync();
foreach (var p in productos)
{
    Console.WriteLine($"  [{p.Id}] {p.Nombre} - {p.Precio:C} ({p.Categoria})");
}

// READ BY ID
Console.WriteLine("\n--- READ BY ID ---");
var encontrado = await repository.GetByIdAsync(nuevoProducto.Id);
Console.WriteLine(encontrado is not null
    ? $"  Encontrado: [{encontrado.Id}] {encontrado.Nombre}"
    : "  No encontrado");

// DELETE
Console.WriteLine("\n--- DELETE ---");
var eliminado = await repository.DeleteAsync(nuevoProducto.Id);
Console.WriteLine(eliminado
    ? $"  Eliminado producto con ID {nuevoProducto.Id}"
    : $"  No se pudo eliminar producto con ID {nuevoProducto.Id}");

Console.WriteLine("\n=== Fin del ejemplo ===");
