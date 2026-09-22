using System.Diagnostics;
using _13_ProgramacionAsincrona.Repositories;
using _13_ProgramacionAsincrona.Services;

// ============================================================
// Ejemplo 13: Programación Asíncrona
// Async/Await vs IAsyncEnumerable vs Rx.NET
// ============================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("=== Ejemplo 13: Programación Asíncrona ===\n");

// Buscar CSV
string[] searchPaths = [
    Path.Combine(AppContext.BaseDirectory, "data", "2025_Accidentalidad.csv"),
    Path.Combine(Directory.GetCurrentDirectory(), "data", "2025_Accidentalidad.csv"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "data", "2025_Accidentalidad.csv"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "data", "2025_Accidentalidad.csv")
];

var rutaCsv = searchPaths.FirstOrDefault(File.Exists) ?? string.Empty;

if (string.IsNullOrEmpty(rutaCsv))
{
    Console.WriteLine("ERROR: No se encontró el fichero CSV en ninguna ubicación.");
    return;
}

var repository = new AccidentesCsvRepository(rutaCsv);

// ── 1. Async/Await ──────────────────────────────────────
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("  1. ASYNC/AWAIT — List<T>");
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("  Lee TODO el CSV de golpe → List<T> → procesa\n");

var service1 = new AsyncAwaitService(repository);
var sw1 = Stopwatch.StartNew();

var total1 = await service1.ContarTotalAsync();
var alcohol1 = await service1.FiltrarAlcoholAsync();

sw1.Stop();
Console.WriteLine($"  Total registros: {total1:N0}");
Console.WriteLine($"  Con alcohol: {alcohol1.Count:N0}");
Console.WriteLine($"  Tiempo: {sw1.ElapsedMilliseconds} ms");
Console.WriteLine("  Memoria: toda la lista en RAM\n");

// ── 2. IAsyncEnumerable ─────────────────────────────────
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("  2. IASYNCENUMERABLE — yield línea a línea");
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("  Lee línea a línea → yield → procesa uno a uno\n");

var service2 = new AsyncEnumerableService(repository);
var sw2 = Stopwatch.StartNew();

var (total2, alcohol2) = await service2.ContarYFiltrarAsync();

sw2.Stop();
Console.WriteLine($"  Total registros: {total2:N0}");
Console.WriteLine($"  Con alcohol: {alcohol2:N0}");
Console.WriteLine($"  Tiempo: {sw2.ElapsedMilliseconds} ms");
Console.WriteLine("  Memoria: solo 1 registro a la vez\n");

// ── 3. Rx.NET ──────────────────────────────────────────
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("  3. RX.NET — Observable");
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("  Lee línea a línea → OnNext → suscriptores filtran\n");

var service3 = new ReactiveService(repository);
var sw3 = Stopwatch.StartNew();

var (total3, alcohol3) = await service3.ProcesarObservableAsync();

sw3.Stop();
Console.WriteLine($"  Total registros: {total3:N0}");
Console.WriteLine($"  Con alcohol: {alcohol3:N0}");
Console.WriteLine($"  Tiempo: {sw3.ElapsedMilliseconds} ms");
Console.WriteLine("  Memoria: solo 1 registro a la vez\n");

// ── Comparativa ──────────────────────────────────────────
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("  COMPARATIVA");
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine($"  Async/Await:        {sw1.ElapsedMilliseconds,6} ms  (carga todo)");
Console.WriteLine($"  IAsyncEnumerable:   {sw2.ElapsedMilliseconds,6} ms  (línea a línea)");
Console.WriteLine($"  Rx.NET:             {sw3.ElapsedMilliseconds,6} ms  (observable)");
Console.WriteLine();
Console.WriteLine("  ¿Cuándo usar cada uno?");
Console.WriteLine("  • Async/Await: datos pequeños, necesitas toda la lista");
Console.WriteLine("  • IAsyncEnumerable: CSV grandes, procesar bajo demanda");
Console.WriteLine("  • Rx.NET: datos en tiempo real, múltiples consumidores");
