using System.Diagnostics;

Console.OutputEncoding = System.Text.Encoding.UTF8;

const int MaxTime = 600;

// ============================================================================
// EJEMPLO 02: DESAYUNO ASÍNCRONO
// ============================================================================
// 7 acciones con sus tiempos:
//   1. Hacer café            (200ms)
//   2. Calentar sartén       (200ms)
//   3. Freír huevos          (300ms)
//   4. Freír bacon           (300ms)
//   5. Tostar pan            (200ms)
//   6. Untar mantequilla     (100ms)
//   7. Hacer zumo            (200ms)
//
// 5 modos de ejecución:
//   PARTE 1: Secuencial           → 1500ms
//   PARTE 2: Asíncrono malo       → 1500ms (solo await, sin paralelismo)
//   PARTE 3: Asíncrono bueno      →  500ms (Task.WhenAll con grupos)
//   PARTE 4: Async malo + CT 500ms →  FALLA (1500ms > 500ms)
//   PARTE 5: Async bueno + CT 500ms → PASA (500ms ≤ 500ms)
// ============================================================================

var sw = new Stopwatch();
var tiempos = new List<(string Modo, long Ms, bool OK)>();

// ============================================================================
// PARTE 1: EJECUCIÓN SECUENCIAL
// ============================================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  PARTE 1: EJECUCIÓN SECUENCIAL");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  Cada acción espera a que termine la anterior.");
Console.WriteLine("  Tiempo total = suma de todos los tiempos.\n");

sw.Start();

await HacerCafe();
await CalentarSarten();
await FreirHuevos();
await FreirBacon();
await TostarPan();
await UntarMantequilla();
await HacerZumo();

sw.Stop();
tiempos.Add(("Secuencial", sw.ElapsedMilliseconds, true));

Console.WriteLine($"\n  ⏱ Tiempo total: {sw.ElapsedMilliseconds}ms\n");

// ============================================================================
// PARTE 2: ASÍNCRONO MALO (solo await, sin paralelismo)
// ============================================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  PARTE 2: ASÍNCRONO MALO");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  Usamos async/await pero cada tarea espera a la anterior.");
Console.WriteLine("  ¡NO hay paralelismo! Es igual de lento que el secuencial.\n");

sw.Restart();

await HacerCafeAsync();
await CalentarSartenAsync();
await FreirHuevosAsync();
await FreirBaconAsync();
await TostarPanAsync();
await UntarMantequillaAsync();
await HacerZumoAsync();

sw.Stop();
tiempos.Add(("Async malo", sw.ElapsedMilliseconds, true));

Console.WriteLine($"\n  ⏱ Tiempo total: {sw.ElapsedMilliseconds}ms\n");

// ============================================================================
// PARTE 3: ASÍNCRONO BUENO (Task.WhenAll con grupos)
// ============================================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  PARTE 3: ASÍNCRONO BUENO");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  Grupo paralelo: 1, A, B, 7");
Console.WriteLine("    ├── 1. Café (200ms)");
Console.WriteLine("    ├── A = secuencial(2, WhenAll(3,4)) = 500ms");
Console.WriteLine("    ├── B = secuencial(5, 6) = 300ms");
Console.WriteLine("    └── 7. Zumo (200ms)");
Console.WriteLine("  Tiempo total = máx(1, A, B, 7) = 500ms\n");

sw.Restart();

// Lanzar los 4 grupos en paralelo
var cafe = HacerCafeAsync();

// Grupo A: secuencial(2, WhenAll(3,4)) = 200 + 300 = 500ms
var GrupoA = EjecutarGrupoA();

// Grupo B: secuencial(5, 6) = 200 + 100 = 300ms
var GrupoB = EjecutarGrupoB();

var zumo = HacerZumoAsync();

// Esperar a que todos los grupos terminen
await Task.WhenAll(cafe, GrupoA, GrupoB, zumo);

sw.Stop();
tiempos.Add(("Async bueno", sw.ElapsedMilliseconds, true));

Console.WriteLine($"\n  ⏱ Tiempo total: {sw.ElapsedMilliseconds}ms\n");

// ============================================================================
// PARTE 4: ASÍNCRONO MALO + CancellationToken (500ms) → FALLA
// ============================================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  PARTE 4: ASÍNCRONO MALO + CancellationToken (500ms)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  Modo secuencial con timeout de 500ms.");
Console.WriteLine("  Necesita 1500ms pero solo tenemos 500ms → ¡FALLA!\n");

using var cts1 = new CancellationTokenSource();
cts1.CancelAfter(MaxTime);

var fallado = false;
try
{
    sw.Restart();

    await HacerCafeAsync(cts1.Token);
    await CalentarSartenAsync(cts1.Token);
    await FreirHuevosAsync(cts1.Token);
    await FreirBaconAsync(cts1.Token);
    await TostarPanAsync(cts1.Token);
    await UntarMantequillaAsync(cts1.Token);
    await HacerZumoAsync(cts1.Token);

    sw.Stop();
}
catch (OperationCanceledException)
{
    sw.Stop();
    fallado = true;
    Console.WriteLine($"  ❌ ¡CANCELADO! El café se ha enfriado. ☕❄️");
    Console.WriteLine($"  Los huevos y tostadas con café frío no tienen gracia...");
    Console.WriteLine($"  Tiempo transcurrido: {sw.ElapsedMilliseconds}ms (límite: 500ms)\n");
}

tiempos.Add(("Async malo + CT", sw.ElapsedMilliseconds, !fallado));

// ============================================================================
// PARTE 5: ASÍNCRONO BUENO + CancellationToken (500ms) → PASA
// ============================================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  PARTE 5: ASÍNCRONO BUENO + CancellationToken (500ms)");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  Modo paralelo con timeout de 500ms.");
Console.WriteLine("  Necesita 500ms y tenemos 500ms → ¡PASA!\n");

using var cts2 = new CancellationTokenSource();
cts2.CancelAfter(MaxTime);

var exito = false;
try
{
    sw.Restart();

    var cafe2 = HacerCafeAsync(cts2.Token);
    var GrupoA2 = EjecutarGrupoA(cts2.Token);
    var GrupoB2 = EjecutarGrupoB(cts2.Token);
    var zumo2 = HacerZumoAsync(cts2.Token);

    await Task.WhenAll(cafe2, GrupoA2, GrupoB2, zumo2);

    sw.Stop();
    exito = true;
    Console.WriteLine($"  ✅ ¡A tiempo! Café calentito, huevos perfectos. ☕🍳");
    Console.WriteLine($"  Tiempo: {sw.ElapsedMilliseconds}ms (límite: 500ms)\n");
}
catch (OperationCanceledException)
{
    sw.Stop();
    Console.WriteLine($"  ❌ ¡CANCELADO! Algo fue demasiado lento.\n");
}

tiempos.Add(("Async bueno + CT", sw.ElapsedMilliseconds, exito));

// ============================================================================
// TABLA COMPARATIVA DE TIEMPOS
// ============================================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  TABLA COMPARATIVA DE TIEMPOS");
Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

Console.WriteLine($"  {"Modo",-25} {"Tiempo",10} {"Estado",10} {"¿Cumple 500ms?",15}");
Console.WriteLine($"  {"─",-25} {"─",10} {"─",10} {"─",15}");

foreach (var (modo, ms, ok) in tiempos)
{
    var estado = ok ? "✅ OK" : "❌ FALLO";
    var cumple = ms <= MaxTime ? "✅ SÍ" : "❌ NO";
    Console.WriteLine($"  {modo,-25} {ms,7}ms {estado,10} {cumple,15}");
}

Console.WriteLine($"\n  📊 Resumen:");
Console.WriteLine($"     • Secuencial: {tiempos[0].Ms}ms");
Console.WriteLine($"     • Async bueno: {tiempos[2].Ms}ms → {tiempos[0].Ms / (double)tiempos[2].Ms:F1}x más rápido");
Console.WriteLine($"     • Async malo + CT: {(tiempos[3].OK ? "PASA" : "FALLA")} ({tiempos[3].Ms}ms vs 500ms)");
Console.WriteLine($"     • Async bueno + CT: {(tiempos[4].OK ? "PASA" : "FALLA")} ({tiempos[4].Ms}ms vs 500ms)\n");

// ============================================================================
// LECCIÓN CLAVE
// ============================================================================
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  LECCIÓN CLAVE");
Console.WriteLine("═══════════════════════════════════════════════════════════════");
Console.WriteLine("  1. async/await SOLO no mejora el rendimiento");
Console.WriteLine("  2. El DISEÑO de qué paralelizamos es lo que marca la diferencia");
Console.WriteLine("  3. CancellationToken permite abortar si superamos un tiempo límite");
Console.WriteLine("  4. Un café frío a las 7am es un drama. Un café a tiempo es vida. ☕\n");

Console.WriteLine("=== Fin del desayuno ===");

// ============================================================================
// MÉTODOS AUXILIARES (7 acciones del desayuno)
// ============================================================================

static async Task HacerCafe(CancellationToken token = default)
{
    Console.WriteLine("  ☕ Café → Encendiendo cafetera...");
    await Task.Delay(200, token);
    Console.WriteLine("  ☕ Café → Listo ✓");
}

static async Task CalentarSarten(CancellationToken token = default)
{
    Console.WriteLine("  🍳 Sartén → Calentando...");
    await Task.Delay(200, token);
    Console.WriteLine("  🍳 Sartén → Lista ✓");
}

static async Task FreirHuevos(CancellationToken token = default)
{
    Console.WriteLine("  🍳 Huevos → friendo...");
    await Task.Delay(300, token);
    Console.WriteLine("  🍳 Huevos → Listos ✓");
}

static async Task FreirBacon(CancellationToken token = default)
{
    Console.WriteLine("  🥓 Bacon → friendo...");
    await Task.Delay(300, token);
    Console.WriteLine("  🥓 Bacon → Listo ✓");
}

static async Task TostarPan(CancellationToken token = default)
{
    Console.WriteLine("  🍞 Pan → Tostando...");
    await Task.Delay(200, token);
    Console.WriteLine("  🍞 Pan → Tostado ✓");
}

static async Task UntarMantequilla(CancellationToken token = default)
{
    Console.WriteLine("  🧈 Mantequilla → Untando...");
    await Task.Delay(100, token);
    Console.WriteLine("  🧈 Mantequilla → Lista ✓");
}

static async Task HacerZumo(CancellationToken token = default)
{
    Console.WriteLine("  🍊 Zumo → Exprimiendo...");
    await Task.Delay(200, token);
    Console.WriteLine("  🍊 Zumo → Listo ✓");
}

// Versiones Async (misma lógica, con async/await explícito)
static async Task HacerCafeAsync(CancellationToken token = default) => await HacerCafe(token);
static async Task CalentarSartenAsync(CancellationToken token = default) => await CalentarSarten(token);
static async Task FreirHuevosAsync(CancellationToken token = default) => await FreirHuevos(token);
static async Task FreirBaconAsync(CancellationToken token = default) => await FreirBacon(token);
static async Task TostarPanAsync(CancellationToken token = default) => await TostarPan(token);
static async Task UntarMantequillaAsync(CancellationToken token = default) => await UntarMantequilla(token);
static async Task HacerZumoAsync(CancellationToken token = default) => await HacerZumo(token);

// Grupo A: secuencial(2, WhenAll(3,4)) = 200 + 300 = 500ms
static async Task EjecutarGrupoA(CancellationToken token = default)
{
    await CalentarSartenAsync(token);
    await Task.WhenAll(FreirHuevosAsync(token), FreirBaconAsync(token));
}

// Grupo B: secuencial(5, 6) = 200 + 100 = 300ms
static async Task EjecutarGrupoB(CancellationToken token = default)
{
    await TostarPanAsync(token);
    await UntarMantequillaAsync(token);
}
