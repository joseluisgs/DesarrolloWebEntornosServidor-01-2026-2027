using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _04_PostgreSQL_Dapper_EFCore.Config;
using _04_PostgreSQL_Dapper_EFCore.Entity;
using _04_PostgreSQL_Dapper_EFCore.Models;
using _04_PostgreSQL_Dapper_EFCore.Repositories.Base;
using _04_PostgreSQL_Dapper_EFCore.Repositories.Dapper;
using _04_PostgreSQL_Dapper_EFCore.Repositories.EfCore;

// ============================================================
// Ejemplo 03: PostgreSQL con Dapper y EF Core
// ============================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Ejemplo 03: PostgreSQL con Dapper y EF Core ===\n");

// Configurar configuración desde appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var dbConfig = new DatabaseConfig();
configuration.GetSection("DatabaseSettings").Bind(dbConfig);

// Registrar Inyección de Dependencias
var services = new ServiceCollection();
services.AddSingleton(dbConfig);

// Registrar repositorio (Dapper o EF Core según argumento)
var useDapper = args.Length > 0 && args[0].ToLower() == "dapper";

if (useDapper)
{
    services.AddSingleton<IProductoRepository>(new ProductoDapperRepository(dbConfig.ConnectionString));
}
else
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbConfig.ConnectionString));
    services.AddScoped<IProductoRepository, ProductoEfRepository>();
}

var provider = services.BuildServiceProvider();
var repository = provider.GetRequiredService<IProductoRepository>();

var repoType = useDapper ? "Dapper" : "EF Core";
Console.WriteLine($"Usando repositorio: {repoType}\n");

// ============================================================
// ⚠️ LIMPIEZA AUTOMÁTICA — SOLO PARA USO EN CLASE
// Controlada por "DropData": true en appsettings.json.
// NUNCA pongas DropData en true en producción.
// ============================================================
if (dbConfig.DropData)
{
    using (var scope = provider.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Productos RESTART IDENTITY");
        Console.WriteLine("  [LIMPIEZA] Tabla Productos vaciada (DropData=true)\n");
    }
}

// ============================================================
// CRUD de Productos
// ============================================================

// CREATE
Console.WriteLine("--- CREATE ---");
var nuevoProducto = new Producto(0, "Teclado Ergonómico", 129.99m, "Periféricos");
var creado = await repository.CreateAsync(nuevoProducto);
Console.WriteLine($"  Creado: [{creado.Id}] {creado.Nombre} - {creado.Precio:C}");

// READ ALL
Console.WriteLine("\n--- READ ALL ---");
var productos = await repository.GetAllAsync();
foreach (var p in productos)
{
    Console.WriteLine($"  [{p.Id}] {p.Nombre} - {p.Precio:C} ({p.Categoria})");
}

// READ BY ID
Console.WriteLine("\n--- READ BY ID ---");
var encontrado = await repository.GetByIdAsync(creado.Id);
if (encontrado is not null)
{
    Console.WriteLine($"  Encontrado: [{encontrado.Id}] {encontrado.Nombre}");
}

// UPDATE
Console.WriteLine("\n--- UPDATE ---");
var actualizado = creado with { Precio = 119.99m };
var resultado = await repository.UpdateAsync(actualizado);
Console.WriteLine(resultado is not null
    ? $"  Actualizado: [{resultado.Id}] {resultado.Nombre} - {resultado.Precio:C}"
    : "  No encontrado");

// DELETE
Console.WriteLine("\n--- DELETE ---");
var eliminado = await repository.DeleteAsync(creado.Id);
Console.WriteLine(eliminado
    ? $"  Eliminado producto con ID {creado.Id}"
    : $"  No se pudo eliminar producto con ID {creado.Id}");

// Verificar
var productosFinales = await repository.GetAllAsync();
Console.WriteLine($"\nTotal productos después de DELETE: {productosFinales.Count()}");

Console.WriteLine("\n=== Fin del ejemplo ===");
