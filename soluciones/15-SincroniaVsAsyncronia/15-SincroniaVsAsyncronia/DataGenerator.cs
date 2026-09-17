using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;

namespace _15_SincroniaVsAsyncronia;

/// <summary>
/// Genera los tres ficheros/fuentes de datos con productos de ejemplo.
/// Se ejecuta una sola vez al inicio del programa.
/// </summary>
public static class DataGenerator
{
    private static readonly string[] Categorias = ["Electrónica", "Ropa", "Hogar", "Deportes", "Alimentación", "Libros", "Juguetes", "Música"];
    private static readonly string[] Proveedores = ["Distribuciones SL", "ImportGlobal", "Mayorista Plus", "TechSupply", "MegaStock", "Almacén Central"];
    private static readonly Random Rng = new(42);

    public static string CsvPath { get; private set; } = "";
    public static string JsonPath { get; private set; } = "";
    public static string DbPath { get; private set; } = "";

    public static void GenerarDatos(string dataDir)
    {
        Directory.CreateDirectory(dataDir);

        CsvPath = Path.Combine(dataDir, "productos.csv");
        JsonPath = Path.Combine(dataDir, "productos.json");
        DbPath = Path.Combine(dataDir, "productos.db");

        GenerarCsv(CsvPath, 1000);
        GenerarJson(JsonPath, 500);
        GenerarSqlite(DbPath, 100);

        Console.WriteLine($"  CSV:    1000 productos → {Path.GetFileName(CsvPath)}");
        Console.WriteLine($"  JSON:    500 productos → {Path.GetFileName(JsonPath)}");
        Console.WriteLine($"  SQLite:  100 productos → {Path.GetFileName(DbPath)}");
        Console.WriteLine();
    }

    private static Producto GenerarProducto(int id)
    {
        return new Producto
        {
            Id = id,
            Nombre = $"Producto-{id:D5}",
            Categoria = Categorias[Rng.Next(Categorias.Length)],
            Precio = Math.Round(Rng.NextDouble() * 500 + 1, 2),
            Stock = Rng.Next(0, 1000),
            Proveedor = Proveedores[Rng.Next(Proveedores.Length)]
        };
    }

    private static void GenerarCsv(string path, int cantidad)
    {
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

        csv.WriteField("Id"); csv.WriteField("Nombre"); csv.WriteField("Categoria");
        csv.WriteField("Precio"); csv.WriteField("Stock"); csv.WriteField("Proveedor");
        csv.NextRecord();

        for (int i = 1; i <= cantidad; i++)
        {
            var p = GenerarProducto(i);
            csv.WriteField(p.Id); csv.WriteField(p.Nombre); csv.WriteField(p.Categoria);
            csv.WriteField(p.Precio); csv.WriteField(p.Stock); csv.WriteField(p.Proveedor);
            csv.NextRecord();
        }
    }

    private static void GenerarJson(string path, int cantidad)
    {
        var productos = new List<Producto>(cantidad);
        for (int i = 1; i <= cantidad; i++)
            productos.Add(GenerarProducto(i));

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(productos, options));
    }

    private static void GenerarSqlite(string path, int cantidad)
    {
        if (File.Exists(path)) File.Delete(path);

        using var db = new ProductosDbContext(path);
        db.Database.EnsureCreated();

        var productos = new List<Producto>(cantidad);
        for (int i = 1; i <= cantidad; i++)
            productos.Add(GenerarProducto(i));

        db.Productos.AddRange(productos);
        db.SaveChanges();
    }
}
