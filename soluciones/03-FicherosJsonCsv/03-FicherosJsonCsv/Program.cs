using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using _03_FicherosJsonCsv.Models;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ============================================================
// Ejemplo 02: Ficheros JSON y CSV
// ============================================================
Console.WriteLine("=== Ejemplo 02: Ficheros JSON y CSV ===\n");

var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(outputDir);

// ============================================================
// PARTE 1: Leer fichero JSON
// ============================================================
Console.WriteLine("--- PARTE 1: Leer productos.json ---");
var jsonPath = Path.Combine(dataDir, "productos.json");
var jsonContent = await File.ReadAllTextAsync(jsonPath);
var productosJson = JsonSerializer.Deserialize<List<Producto>>(jsonContent, new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
}) ?? [];

foreach (var p in productosJson)
{
    Console.WriteLine($"  [{p.Id}] {p.Nombre} - {p.Precio:C} ({p.Categoria})");
}

// ============================================================
// PARTE 2: Leer fichero CSV
// ============================================================
Console.WriteLine("\n--- PARTE 2: Leer productos.csv ---");
var csvPath = Path.Combine(dataDir, "productos.csv");
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
    MissingFieldFound = null,
    BadDataFound = null
};

using (var reader = new StreamReader(csvPath))
using (var csv = new CsvReader(reader, config))
{
    var productosCsv = csv.GetRecords<Producto>().ToList();
    foreach (var p in productosCsv)
    {
        Console.WriteLine($"  [{p.Id}] {p.Nombre} - {p.Precio:C} ({p.Categoria})");
    }
}

// ============================================================
// PARTE 3: Exportar a JSON
// ============================================================
Console.WriteLine("\n--- PARTE 3: Exportar a JSON ---");
var todosLosProductos = productosJson.Union(productosJson).DistinctBy(p => p.Id).ToList();

var opcionesJson = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

var jsonExport = JsonSerializer.Serialize(todosLosProductos, opcionesJson);
var jsonExportPath = Path.Combine(outputDir, "productos_export.json");
await File.WriteAllTextAsync(jsonExportPath, jsonExport);
Console.WriteLine($"  Exportado a: {jsonExportPath}");
Console.WriteLine($"  Total productos: {todosLosProductos.Count}");

// ============================================================
// PARTE 4: Exportar a CSV
// ============================================================
Console.WriteLine("\n--- PARTE 4: Exportar a CSV ---");
var csvExportPath = Path.Combine(outputDir, "productos_export.csv");

using (var writer = new StreamWriter(csvExportPath))
using (var csv = new CsvWriter(writer, config))
{
    csv.WriteRecords(todosLosProductos);
}

Console.WriteLine($"  Exportado a: {csvExportPath}");
Console.WriteLine($"  Total productos: {todosLosProductos.Count}");

Console.WriteLine("\n=== Fin del ejemplo ===");
