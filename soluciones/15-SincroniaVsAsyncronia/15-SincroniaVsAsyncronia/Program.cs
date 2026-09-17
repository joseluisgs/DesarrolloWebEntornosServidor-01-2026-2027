using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using _15_SincroniaVsAsyncronia;

Console.OutputEncoding = Encoding.UTF8;

// ============================================================================
// EJEMPLO: Sincronía vs Asíncronía — Tres enfoques para leer datos
// ============================================================================
// Fuentes de datos:
//   - CSV:   1000 productos (fichero)
//   - JSON:   500 productos (fichero)
//   - SQLite: 100 productos (EF Core)
//
// Tres enfoques:
//   1. SÍNCRONO:      Lee todo secuencialmente (bloquea el hilo)
//   2. ASYNC MAL:     await secuencial (NO es paralelismo)
//   3. ASYNC BIEN:    Task.WhenAll + await (paralelo de verdad)
//
// El ILogger muestra el THREAD ID de cada operación para que se vea
// claramente cuándo se ejecutan en paralelo y cuándo no.
// ============================================================================

// ── SERILOG + ILogger ────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] Thread:{ThreadId} {Message}{NewLine}")
    .CreateLogger();

using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddSerilog(dispose: true));
var logger = loggerFactory.CreateLogger<Program>();

var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
var sw = new Stopwatch();

Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║   Sincronía vs Asíncronía — Tres enfoques para leer datos  ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("  🔍 Fíjate en el THREAD ID: mismo hilo = secuencial, distintos = paralelo");
Console.WriteLine();

// ── PASO 1: Generar datos ────────────────────────────────────
logger.LogInformation("📦 Generando datos de ejemplo...");
DataGenerator.GenerarDatos(dataDir);

// ============================================================================
// 1. SÍNCRONO — Secuencial, bloqueante
// ============================================================================
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("  1️⃣  SÍNCRONO — Todo secuencial, hilo bloqueado");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();

sw.Restart();
var csvSync = LeerCsvSync(DataGenerator.CsvPath);
var jsonSync = LeerJsonSync(DataGenerator.JsonPath);
var sqliteSync = LeerSqliteSync(DataGenerator.DbPath);
sw.Stop();

Console.WriteLine();
Console.WriteLine($"  CSV:    {csvSync.Count} productos leídos");
Console.WriteLine($"  JSON:   {jsonSync.Count} productos leídos");
Console.WriteLine($"  SQLite: {sqliteSync.Count} productos leídos");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"  ⏱  TIEMPO TOTAL SÍNCRONO: {sw.ElapsedMilliseconds} ms");
Console.ResetColor();
Console.WriteLine($"  🧵  Mismo thread ID en las tres operaciones = secuencial");
Console.WriteLine();

// ============================================================================
// 2. ASYNC MAL — Await secuencial (NO es paralelismo)
// ============================================================================
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("  2️⃣  ASYNC MAL — Await secuencial (misma diferencia que sync)");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
Console.WriteLine("  ⚠️  ERROR común: cada await ESPERA a que termine el anterior.");
Console.WriteLine("     NO hay paralelismo. Es igual de lento que síncrono.");
Console.WriteLine();

sw.Restart();
var csvAsync = await LeerCsvAsync(DataGenerator.CsvPath);
var jsonAsync = await LeerJsonAsync(DataGenerator.JsonPath);
var sqliteAsync = await LeerSqliteAsync(DataGenerator.DbPath);
sw.Stop();

Console.WriteLine();
Console.WriteLine($"  CSV:    {csvAsync.Count} productos leídos");
Console.WriteLine($"  JSON:   {jsonAsync.Count} productos leídos");
Console.WriteLine($"  SQLite: {sqliteAsync.Count} productos leídos");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine($"  ⏱  TIEMPO TOTAL ASYNC MAL: {sw.ElapsedMilliseconds} ms");
Console.ResetColor();
Console.WriteLine($"  🧵  Secuencial. Cada await espera al anterior. Un solo hilo haciendo todo");
Console.WriteLine();

// ============================================================================
// 3. ASYNC BIEN — Task.WhenAll (paralelo de verdad)
// ============================================================================
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("  3️⃣  ASYNC BIEN — Lanzamos los tres en paralelo con WhenAll");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
Console.WriteLine("  ✅ Lanzamos las TAREAS sin await. Se ejecutan en paralelo.");
Console.WriteLine("     Luego esperamos con Task.WhenAll. Mucho más rápido.");
Console.WriteLine();

sw.Restart();
var tareaCsv = LeerCsvAsync(DataGenerator.CsvPath);
var tareaJson = LeerJsonAsync(DataGenerator.JsonPath);
var tareaSqlite = LeerSqliteAsync(DataGenerator.DbPath);

await Task.WhenAll(tareaCsv, tareaJson, tareaSqlite);

var csvBien = tareaCsv.Result;
var jsonBien = tareaJson.Result;
var sqliteBien = tareaSqlite.Result;
sw.Stop();

Console.WriteLine();
Console.WriteLine($"  CSV:    {csvBien.Count} productos leídos");
Console.WriteLine($"  JSON:   {jsonBien.Count} productos leídos");
Console.WriteLine($"  SQLite: {sqliteBien.Count} productos leídos");
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"  ⏱  TIEMPO TOTAL ASYNC BIEN: {sw.ElapsedMilliseconds} ms");
Console.ResetColor();
Console.WriteLine($"  🧵  Distintos thread IDs = ejecución paralela");
Console.WriteLine();

// ============================================================================
// RESUMEN
// ============================================================================
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("  📊  RESUMEN COMPARATIVO");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine();
Console.WriteLine("  ┌─────────────────┬────────────────────────────────────────┐");
Console.WriteLine("  │ Síncrono        │ Todo secuencial, hilo bloqueado       │");
Console.WriteLine("  │ Async MAL       │ await secuencial = MISMO que sync     │");
Console.WriteLine("  │ Async BIEN      │ Task.WhenAll = paralelo de verdad    │");
Console.WriteLine("  └─────────────────┴────────────────────────────────────────┘");
Console.WriteLine();

// ============================================================================
// MÉTODOS DE LECTURA
// ============================================================================

// ── CSV ──────────────────────────────────────────────────────
List<Producto> LeerCsvSync(string path)
{
    logger.LogDebug("📂 CSV Sync: leyendo {Path} en Thread {ThreadId}", path, Environment.CurrentManagedThreadId);
    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
    var records = csv.GetRecords<Producto>().ToList();
    logger.LogDebug("📂 CSV Sync: fin ({Count} productos) Thread {ThreadId}", records.Count, Environment.CurrentManagedThreadId);
    return records;
}

async Task<List<Producto>> LeerCsvAsync(string path)
{
    logger.LogDebug("📂 CSV Async: iniciando lectura en Thread {ThreadId}", Environment.CurrentManagedThreadId);
    await Task.Delay(1); // Simula I/O asíncrono real
    using var reader = new StreamReader(path);
    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
    var records = new List<Producto>();
    await foreach (var record in csv.GetRecordsAsync<Producto>())
        records.Add(record);
    logger.LogDebug("📂 CSV Async: fin ({Count} productos) Thread {ThreadId}", records.Count, Environment.CurrentManagedThreadId);
    return records;
}

// ── JSON ─────────────────────────────────────────────────────
List<Producto> LeerJsonSync(string path)
{
    logger.LogDebug("📄 JSON Sync: leyendo {Path} en Thread {ThreadId}", path, Environment.CurrentManagedThreadId);
    var json = File.ReadAllText(path);
    return System.Text.Json.JsonSerializer.Deserialize<List<Producto>>(json) ?? [];
}

async Task<List<Producto>> LeerJsonAsync(string path)
{
    logger.LogDebug("📄 JSON Async: iniciando lectura en Thread {ThreadId}", Environment.CurrentManagedThreadId);
    await Task.Delay(1); // Simula I/O asíncrono real
    using var stream = File.OpenRead(path);
    return await System.Text.Json.JsonSerializer.DeserializeAsync<List<Producto>>(stream) ?? [];
}

// ── SQLITE (EF Core) ────────────────────────────────────────
List<Producto> LeerSqliteSync(string path)
{
    logger.LogDebug("🗄️  SQLite Sync: leyendo en Thread {ThreadId}", Environment.CurrentManagedThreadId);
    using var db = new ProductosDbContext(path);
    return db.Productos.ToList();
}

async Task<List<Producto>> LeerSqliteAsync(string path)
{
    logger.LogDebug("🗄️  SQLite Async: iniciando lectura en Thread {ThreadId}", Environment.CurrentManagedThreadId);
    await Task.Delay(1); // Simula I/O asíncrono real
    using var db = new ProductosDbContext(path);
    return await db.Productos.ToListAsync();
}
