using AccidentesMadrid.Repositories;
using AccidentesMadrid.Services;
using System.Diagnostics;
using Serilog;

// ============================================================================
// ANÁLISIS DE ACCIDENTES DE MADRID
// Comparativa: E/S Paralela vs Cálculo Paralelo (dos motores)
// ============================================================================
// LECCIÓN CLAVE:
// - E/S (lectura de ficheros) → Paralelizar SÍ suele mejorar: la E/S espera
//   (disco, red), y mientras un hilo espera otro puede trabajar.
// - Cálculo en memoria → DEPENDE de los recursos libres. Si hay núcleos de CPU
//   disponibles y cada tarea tarda lo suficiente, mejora. Si la CPU está ocupada
//   o las tareas son pequeñas, el overhead puede hacerlo peor.
//   NO es siempre peor: hay que MEDIR en cada caso.
// - Motores: Task.WhenAll (async, 1 Task por consulta o por lote) frente a
//   Parallel.For (bloqueante, reparto de iteraciones). Mismo ThreadPool,
//   misma forma de valle: el óptimo de granularidad se busca midiendo.
//
// Serilog muestra el THREAD ID para ver en qué hilo se ejecuta cada cosa.
// ============================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] Thread:{ThreadId} {Message}{NewLine}")
    .CreateLogger();

var dataDir = Path.Combine(AppContext.BaseDirectory, "data");
if (!Directory.Exists(dataDir))
    dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");

var ficheros = new (string anio, string ruta)[]
{
    ("2024", Path.Combine(dataDir, "2024_Accidentalidad.csv")),
    ("2025", Path.Combine(dataDir, "2025_Accidentalidad.csv")),
    ("2026", Path.Combine(dataDir, "2026_Accidentalidad.csv"))
};

foreach (var (anio, ruta) in ficheros)
{
    if (!File.Exists(ruta))
    {
        Console.WriteLine($"ERROR: No se encontró {Path.GetFileName(ruta)} en {dataDir}");
        Console.WriteLine("Descarga los ficheros de https://datos.madrid.es y colócalos en data/");
        return;
    }
}

Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  ANÁLISIS DE ACCIDENTES DE MADRID                            ║");
Console.WriteLine("║  E/S Paralela vs Cálculo Paralelo                           ║");
Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ══════════════════════════════════════════════════════════════════
// FASE 1: LECTURA DE FICHEROS (E/S)
// ══════════════════════════════════════════════════════════════════

// ── 1A. Lectura SECUENCIAL ─────────────────────────────────────
Log.Information("═══ FASE 1: LECTURA DE FICHEROS (E/S) ═══");
Console.WriteLine();

Log.Information("Lectura SECUENCIAL (uno detrás de otro)...");
var swSeq = Stopwatch.StartNew();

var repoSeq1 = new AccidentesRepository(ficheros[0].ruta);
var accidentesSeq1 = await repoSeq1.GetAllAsync();
Log.Information("  2024: {Count} registros", accidentesSeq1.Count);

var repoSeq2 = new AccidentesRepository(ficheros[1].ruta);
var accidentesSeq2 = await repoSeq2.GetAllAsync();
Log.Information("  2025: {Count} registros", accidentesSeq2.Count);

var repoSeq3 = new AccidentesRepository(ficheros[2].ruta);
var accidentesSeq3 = await repoSeq3.GetAllAsync();
Log.Information("  2026: {Count} registros", accidentesSeq3.Count);

swSeq.Stop();
Log.Information("  ⏱ Secuencial: {Ms} ms", swSeq.ElapsedMilliseconds);
Console.WriteLine();

// ── 1B. Lectura PARALELA ──────────────────────────────────────
Log.Information("Lectura PARALELA (las 3 a la vez)...");
var swPar = Stopwatch.StartNew();

var repoPar1 = new AccidentesRepository(ficheros[0].ruta);
var repoPar2 = new AccidentesRepository(ficheros[1].ruta);
var repoPar3 = new AccidentesRepository(ficheros[2].ruta);

// LANZAMOS sin await — se ejecutan en paralelo
var tareaPar1 = repoPar1.GetAllAsync();
var tareaPar2 = repoPar2.GetAllAsync();
var tareaPar3 = repoPar3.GetAllAsync();

Log.Information("  3 tareas lanzadas, esperando con Task.WhenAll...");
await Task.WhenAll(tareaPar1, tareaPar2, tareaPar3);

var accidentesPar1 = tareaPar1.Result;
var accidentesPar2 = tareaPar2.Result;
var accidentesPar3 = tareaPar3.Result;

swPar.Stop();

Log.Information("  2024: {Count} registros", accidentesPar1.Count);
Log.Information("  2025: {Count} registros", accidentesPar2.Count);
Log.Information("  2026: {Count} registros", accidentesPar3.Count);
Log.Information("  ⏱ Paralelo: {Ms} ms", swPar.ElapsedMilliseconds);
Console.WriteLine();

// ── Combinar datos ─────────────────────────────────────────────
var todosLosAccidentes = accidentesPar1
    .Concat(accidentesPar2)
    .Concat(accidentesPar3)
    .ToList();

Log.Information("  TOTAL: {Count} registros combinados", todosLosAccidentes.Count);

var gananciaEscriba = swSeq.ElapsedMilliseconds > 0
    ? (double)swSeq.ElapsedMilliseconds / swPar.ElapsedMilliseconds
    : 0;
var veredictoES = gananciaEscriba > 1
    ? "SÍ mejora (la E/S espera: mientras un hilo espera, otro trabaja)"
    : "NO mejora en esta ejecución";
Log.Information("  Speedup E/S: {Ratio:F2}x — {Resultado}", gananciaEscriba, veredictoES);
Console.WriteLine();

// ══════════════════════════════════════════════════════════════════
// FASE 2: CONSULTAS LINQ (CÁLCULO EN MEMORIA)
// ══════════════════════════════════════════════════════════════════
Log.Information("═══ FASE 2: CONSULTAS LINQ (CÁLCULO EN MEMORIA) ═══");
Console.WriteLine();

// Variantes por lote: número de tareas. Cada una divide 30 en partes iguales
// (2→15, 3→10, 5→6, 6→5, 10→3, 15→2 consultas por tarea): ni 1 tarea, ni 30.
int[] VariantesLotes = [3, 10, 5, 6, 2, 15];

// ── 2A. Consultas SECUENCIALES ─────────────────────────────────
Log.Information("LINQ SECUENCIAL...");
var swLinqSeq = Stopwatch.StartNew();
var linqAnalyzer = new AccidentesLinqAnalyzer();
linqAnalyzer.EjecutarConsultas(todosLosAccidentes);
swLinqSeq.Stop();
Log.Information("  ⏱ LINQ secuencial: {Ms} ms", swLinqSeq.ElapsedMilliseconds);
Console.WriteLine();

// ── 2B. Consultas PARALELAS ────────────────────────────────────
Log.Information("LINQ PARALELO (Task.WhenAll)...");
var swLinqPar = Stopwatch.StartNew();
await linqAnalyzer.EjecutarConsultasParalelasAsync(todosLosAccidentes);
swLinqPar.Stop();
Log.Information("  ⏱ LINQ paralelo: {Ms} ms", swLinqPar.ElapsedMilliseconds);
Console.WriteLine();

var gananciaLinq = swLinqSeq.ElapsedMilliseconds > 0
    ? (double)swLinqSeq.ElapsedMilliseconds / swLinqPar.ElapsedMilliseconds
    : 0;
var veredictoLinq = gananciaLinq > 1
    ? "SÍ mejora en esta ejecución (núcleos libres + consultas con recorrido)"
    : "NO mejora en esta ejecución (overhead de Task.Run > beneficio)";
Log.Information("  Speedup LINQ: {Ratio:F2}x — {Resultado}", gananciaLinq, veredictoLinq);
Console.WriteLine();

// ── 2C. Consultas por LOTES (granularidad intermedia) ──────────
// ¿Por qué estos números? Porque 30 consultas deben repartirse
// en partes IGUALES: 30/3=10, 30/10=3, 30/5=6, 30/6=5, 30/2=15, 30/15=2.
Log.Information("LINQ POR LOTES (3×10, 10×3, 5×6, 6×5, 2×15, 15×2)...");
var msLinqLotes = new Dictionary<int, long>();
foreach (var numeroTareas in VariantesLotes)
{
    var swLote = Stopwatch.StartNew();
    await linqAnalyzer.EjecutarConsultasEnLotesAsync(todosLosAccidentes, numeroTareas);
    swLote.Stop();
    msLinqLotes[numeroTareas] = swLote.ElapsedMilliseconds;
    Log.Information("  ⏱ {Tareas} tareas × {Consultas} consultas: {Ms} ms",
        numeroTareas, 30 / numeroTareas, swLote.ElapsedMilliseconds);
}
Console.WriteLine();

// ══════════════════════════════════════════════════════════════════
// FASE 3: CONSULTAS DATAFRAMES
// ══════════════════════════════════════════════════════════════════
Log.Information("═══ FASE 3: CONSULTAS DATAFRAMES ═══");
Console.WriteLine();

// ── 3A. DataFrames SECUENCIALES ────────────────────────────────
Log.Information("DataFrames SECUENCIAL...");
var swDfSeq = Stopwatch.StartNew();
var dfAnalyzer = new AccidentesDataFrameAnalyzer();
dfAnalyzer.EjecutarConsultas(todosLosAccidentes);
swDfSeq.Stop();
Log.Information("  ⏱ DataFrames secuencial: {Ms} ms", swDfSeq.ElapsedMilliseconds);
Console.WriteLine();

// ── 3B. DataFrames PARALELOS ───────────────────────────────────
Log.Information("DataFrames PARALELO...");
var swDfPar = Stopwatch.StartNew();
await dfAnalyzer.EjecutarConsultasParalelasAsync(todosLosAccidentes);
swDfPar.Stop();
Log.Information("  ⏱ DataFrames paralelo: {Ms} ms", swDfPar.ElapsedMilliseconds);
Console.WriteLine();

var gananciaDf = swDfSeq.ElapsedMilliseconds > 0
    ? (double)swDfSeq.ElapsedMilliseconds / swDfPar.ElapsedMilliseconds
    : 0;
var veredictoDf = gananciaDf > 1
    ? "SÍ mejora en esta ejecución (consultas DataFrame pesadas > overhead)"
    : "NO mejora en esta ejecución (overhead de Task.Run > beneficio)";
Log.Information("  Speedup DataFrames: {Ratio:F2}x — {Resultado}", gananciaDf, veredictoDf);
Console.WriteLine();

// ── 3C. DataFrames por LOTES (misma granularidad) ──────────────
Log.Information("DataFrames POR LOTES (3×10, 10×3, 5×6, 6×5, 2×15, 15×2)...");
var msDfLotes = new Dictionary<int, long>();
foreach (var numeroTareas in VariantesLotes)
{
    var swLote = Stopwatch.StartNew();
    await dfAnalyzer.EjecutarConsultasEnLotesAsync(todosLosAccidentes, numeroTareas);
    swLote.Stop();
    msDfLotes[numeroTareas] = swLote.ElapsedMilliseconds;
    Log.Information("  ⏱ {Tareas} tareas × {Consultas} consultas: {Ms} ms",
        numeroTareas, 30 / numeroTareas, swLote.ElapsedMilliseconds);
}
Console.WriteLine();

// ══════════════════════════════════════════════════════════════════
// FASE 4: MISMAS CONSULTAS LINQ, OTRO MOTOR — PARALLEL.FOR
// ══════════════════════════════════════════════════════════════════
Log.Information("═══ FASE 4: CONSULTAS LINQ CON MOTOR PARALLEL.FOR ═══");
Console.WriteLine();

// ── 4A. Parallel.For: una iteración por consulta ───────────────
Log.Information("LINQ con Parallel.For (una iteración por consulta)...");
var swLinqPfPar = Stopwatch.StartNew();
await linqAnalyzer.EjecutarConsultasParallelForAsync(todosLosAccidentes);
swLinqPfPar.Stop();
Log.Information("  ⏱ LINQ Parallel.For ×30: {Ms} ms", swLinqPfPar.ElapsedMilliseconds);

var gananciaLinqPf = swLinqSeq.ElapsedMilliseconds > 0
    ? (double)swLinqSeq.ElapsedMilliseconds / swLinqPfPar.ElapsedMilliseconds
    : 0;
Log.Information("  Speedup LINQ Parallel.For: {Ratio:F2}x", gananciaLinqPf);
Console.WriteLine();

// ── 4B. Parallel.For por LOTES (Partitioner.Create) ────────────
Log.Information("LINQ Parallel.For POR LOTES (3×10, 10×3, 5×6, 6×5, 2×15, 15×2)...");
var msLinqPf = new Dictionary<int, long>();
foreach (var numeroTareas in VariantesLotes)
{
    var swLote = Stopwatch.StartNew();
    await linqAnalyzer.EjecutarConsultasPorLotesParallelForAsync(todosLosAccidentes, numeroTareas);
    swLote.Stop();
    msLinqPf[numeroTareas] = swLote.ElapsedMilliseconds;
    Log.Information("  ⏱ {Tareas} paquetes × {Consultas} consultas: {Ms} ms",
        numeroTareas, 30 / numeroTareas, swLote.ElapsedMilliseconds);
}
Console.WriteLine();

// ══════════════════════════════════════════════════════════════════
// FASE 5: MISMAS CONSULTAS DATAFRAME, OTRO MOTOR — PARALLEL.FOR
// ══════════════════════════════════════════════════════════════════
Log.Information("═══ FASE 5: CONSULTAS DATAFRAMES CON MOTOR PARALLEL.FOR ═══");
Console.WriteLine();

// ── 5A. Parallel.For: una iteración por consulta ───────────────
Log.Information("DataFrames con Parallel.For (una iteración por consulta)...");
var swDfPfPar = Stopwatch.StartNew();
await dfAnalyzer.EjecutarConsultasParallelForAsync(todosLosAccidentes);
swDfPfPar.Stop();
Log.Information("  ⏱ DataFrames Parallel.For ×30: {Ms} ms", swDfPfPar.ElapsedMilliseconds);

var gananciaDfPf = swDfSeq.ElapsedMilliseconds > 0
    ? (double)swDfSeq.ElapsedMilliseconds / swDfPfPar.ElapsedMilliseconds
    : 0;
Log.Information("  Speedup DataFrames Parallel.For: {Ratio:F2}x", gananciaDfPf);
Console.WriteLine();

// ── 5B. Parallel.For por LOTES (Partitioner.Create) ────────────
Log.Information("DataFrames Parallel.For POR LOTES (3×10, 10×3, 5×6, 6×5, 2×15, 15×2)...");
var msDfPf = new Dictionary<int, long>();
foreach (var numeroTareas in VariantesLotes)
{
    var swLote = Stopwatch.StartNew();
    await dfAnalyzer.EjecutarConsultasPorLotesParallelForAsync(todosLosAccidentes, numeroTareas);
    swLote.Stop();
    msDfPf[numeroTareas] = swLote.ElapsedMilliseconds;
    Log.Information("  ⏱ {Tareas} paquetes × {Consultas} consultas: {Ms} ms",
        numeroTareas, 30 / numeroTareas, swLote.ElapsedMilliseconds);
}
Console.WriteLine();

// ══════════════════════════════════════════════════════════════════
// RESUMEN FINAL
// ══════════════════════════════════════════════════════════════════
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  RESUMEN: E/S vs CÁLCULO (las 30 consultas, ×30)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("  FASE 1 — E/S (lectura de ficheros):");
Console.WriteLine($"    Secuencial:  {swSeq.ElapsedMilliseconds,6} ms");
Console.WriteLine($"    Paralelo:    {swPar.ElapsedMilliseconds,6} ms");
Console.WriteLine($"    Speedup:     {gananciaEscriba,6:F2}x  ← {veredictoES}");
Console.WriteLine();
Console.WriteLine("  FASE 2/4 — LINQ (cálculo en memoria):");
Console.WriteLine($"    Secuencial:       {swLinqSeq.ElapsedMilliseconds,6} ms");
Console.WriteLine($"    Task.WhenAll:     {swLinqPar.ElapsedMilliseconds,6} ms  ← {gananciaLinq:F2}x");
Console.WriteLine($"    Parallel.For:     {swLinqPfPar.ElapsedMilliseconds,6} ms  ← {gananciaLinqPf:F2}x");
Console.WriteLine();
Console.WriteLine("  FASE 3/5 — DataFrames (cálculo en memoria):");
Console.WriteLine($"    Secuencial:       {swDfSeq.ElapsedMilliseconds,6} ms");
Console.WriteLine($"    Task.WhenAll:     {swDfPar.ElapsedMilliseconds,6} ms  ← {gananciaDf:F2}x");
Console.WriteLine($"    Parallel.For:     {swDfPfPar.ElapsedMilliseconds,6} ms  ← {gananciaDfPf:F2}x");
Console.WriteLine();

// ── Resumen: granularidad de tareas × motor ────────────────────
// Filas en orden creciente de tareas; dos columnas: un motor cada una.
(string Etiqueta, long? TaskMs, long? PfMs)[] filasLinq =
[
    ("Secuencial:   1 × 30", swLinqSeq.ElapsedMilliseconds, null),
    ("Lotes:        2 × 15", msLinqLotes[2], msLinqPf[2]),
    ("Lotes:        3 × 10", msLinqLotes[3], msLinqPf[3]),
    ("Lotes:        5 × 6", msLinqLotes[5], msLinqPf[5]),
    ("Lotes:        6 × 5", msLinqLotes[6], msLinqPf[6]),
    ("Lotes:       10 × 3", msLinqLotes[10], msLinqPf[10]),
    ("Lotes:       15 × 2", msLinqLotes[15], msLinqPf[15]),
    ("Paralelo:    30 × 1", swLinqPar.ElapsedMilliseconds, swLinqPfPar.ElapsedMilliseconds)
];

(string Etiqueta, long? TaskMs, long? PfMs)[] filasDf =
[
    ("Secuencial:   1 × 30", swDfSeq.ElapsedMilliseconds, null),
    ("Lotes:        2 × 15", msDfLotes[2], msDfPf[2]),
    ("Lotes:        3 × 10", msDfLotes[3], msDfPf[3]),
    ("Lotes:        5 × 6", msDfLotes[5], msDfPf[5]),
    ("Lotes:        6 × 5", msDfLotes[6], msDfPf[6]),
    ("Lotes:       10 × 3", msDfLotes[10], msDfPf[10]),
    ("Lotes:       15 × 2", msDfLotes[15], msDfPf[15]),
    ("Paralelo:    30 × 1", swDfPar.ElapsedMilliseconds, swDfPfPar.ElapsedMilliseconds)
];

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  RESUMEN: ¿CUÁNTAS TAREAS PARA LAS 30 CONSULTAS? (ms)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine();
ImprimirTabla("FASE 2/4 — LINQ", filasLinq);
ImprimirTabla("FASE 3/5 — DataFrames", filasDf);

var mejorGlobalLinq = MejorGlobal(filasLinq);
var mejorGlobalDf = MejorGlobal(filasDf);
Console.WriteLine($"    → Mejor LINQ global:      {mejorGlobalLinq.Motor} · {mejorGlobalLinq.Etiqueta} ({mejorGlobalLinq.Ms} ms)");
Console.WriteLine($"    → Mejor DataFrame global: {mejorGlobalDf.Motor} · {mejorGlobalDf.Etiqueta} ({mejorGlobalDf.Ms} ms)");
Console.WriteLine("    (En otra máquina con otros recursos puede ganar otra variante: MIDE).");
Console.WriteLine();

Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  LECCIÓN:");
Console.WriteLine("  1. E/S → paralelizar SÍ casi siempre mejora (espera disco/red/BD).");
Console.WriteLine("  2. Cálculo en memoria → DEPENDE de los recursos libres:");
Console.WriteLine("     - Núcleos disponibles + consultas pesadas → SÍ mejora (DataFrames).");
Console.WriteLine("     - CPU ocupada o consultas muy baratas     → puede no mejorar (LINQ).");
Console.WriteLine("  3. La granularidad IMPORTA con CUALQUIER motor: ni 1 sola tarea");
Console.WriteLine("     (secuencial), ni 30 (máximo overhead). El óptimo se BUSCA midiendo.");
Console.WriteLine($"  4. Task.WhenAll (async) y Parallel.For (bloqueante) comparten el");
Console.WriteLine($"     ThreadPool y repiten la forma de valle. En ESTA ejecución ganó");
Console.WriteLine($"     {mejorGlobalLinq.Motor} en LINQ y {mejorGlobalDf.Motor} en DataFrames:");
Console.WriteLine($"     el motor también se elige midiendo. En servidor con E/S → async;");
Console.WriteLine("     en consola con cálculo puro, los dos sirven.");
Console.WriteLine("  NO es siempre peor: MIDE en tu máquina antes de decidir.");
Console.WriteLine("═══════════════════════════════════════════════════════════════");

// ── Auxiliares del resumen ─────────────────────────────────────
static void ImprimirTabla(string titulo, (string Etiqueta, long? TaskMs, long? PfMs)[] filas)
{
    Console.WriteLine($"  {titulo}");
    Console.WriteLine($"    {"Variante",-26}{"Task.WhenAll",14}{"Parallel.For",16}");
    foreach (var f in filas)
    {
        var taskTxt = f.TaskMs.HasValue ? $"{f.TaskMs.Value} ms" : "—";
        var pfTxt = f.PfMs.HasValue ? $"{f.PfMs.Value} ms" : "—";
        Console.WriteLine($"    {f.Etiqueta,-26}{taskTxt,14}{pfTxt,16}");
    }

    var mejorTask = filas.Where(f => f.TaskMs.HasValue).OrderBy(f => f.TaskMs).First();
    var mejorPf = filas.Where(f => f.PfMs.HasValue).OrderBy(f => f.PfMs).First();
    Console.WriteLine($"    → Mejor con Task.WhenAll: {mejorTask.Etiqueta} ({mejorTask.TaskMs} ms)");
    Console.WriteLine($"    → Mejor con Parallel.For: {mejorPf.Etiqueta} ({mejorPf.PfMs} ms)");
    Console.WriteLine();
}

static (string Motor, string Etiqueta, long Ms) MejorGlobal((string Etiqueta, long? TaskMs, long? PfMs)[] filas) =>
    filas.Where(f => f.TaskMs.HasValue)
        .Select(f => (Motor: "Task.WhenAll", f.Etiqueta, Ms: f.TaskMs!.Value))
        .Concat(filas.Where(f => f.PfMs.HasValue)
            .Select(f => (Motor: "Parallel.For", f.Etiqueta, Ms: f.PfMs!.Value)))
        .OrderBy(f => f.Ms)
        .First();
