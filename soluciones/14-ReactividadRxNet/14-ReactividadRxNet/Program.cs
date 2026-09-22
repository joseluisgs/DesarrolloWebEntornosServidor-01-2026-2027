using System.Reactive.Linq;
using _14_ReactividadRxNet.Services;

// ============================================================================
// EJEMPLO: Programación Reactiva con Rx.NET — Flujo CALIENTE
// ============================================================================
// Un flujo CALIENTE significa: si un observador se conecta tarde,
// se PIERDE lo que ya pasó. Solo ve eventos desde el momento de su suscripción.
// ============================================================================

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var sensor = new SensorService();
using var cts = new CancellationTokenSource();

var consumidor1Count = 0;
var consumidor2Count = 0;
var consumidor1Terminado = false;
var consumidor2Terminado = false;

// ── CONSUMIDOR 1: solo CREATE y UPDATE (filtra ERROR y DELETE) ──
var consumidor1 = sensor.Observable
    .Where(n => n.Tipo != NotificacionTipo.Error && n.Tipo != NotificacionTipo.Delete)
    .Subscribe(
        n =>
        {
            consumidor1Count++;
            Console.WriteLine(FormatearMensaje("Consumidor-1", n, consumidor1Count));

            if (consumidor1Count >= 30 && !consumidor1Terminado)
            {
                consumidor1Terminado = true;
                Console.WriteLine("\n✅ Consumidor-1 terminó (30 mensajes).");
                ComprobarFin();
            }
        }
    );

// ── CONSUMIDOR 2: solo CREATE, DELETE y ERROR (filtra UPDATE) ──
// Se conecta 8 segundos tarde — solo ve desde AHORA
_ = Task.Run(async () =>
{
    await Task.Delay(8000, cts.Token);

    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("  ⏰ Consumidor-2 se conecta (8 segundos de retraso)");
    Console.WriteLine("  📡 Solo verá eventos desde AHORA. Los anteriores se perdieron.");
    Console.WriteLine("═══════════════════════════════════════════════════════\n");

    sensor.Observable
        .Where(n => n.Tipo != NotificacionTipo.Update)
        .Subscribe(
            n =>
            {
                consumidor2Count++;
                Console.WriteLine(FormatearMensaje("Consumidor-2", n, consumidor2Count));

                if (consumidor2Count >= 30 && !consumidor2Terminado)
                {
                    consumidor2Terminado = true;
                    Console.WriteLine("\n✅ Consumidor-2 terminó (30 mensajes).");
                    ComprobarFin();
                }
            }
        );
});

// ── COMPROBAR SI AMBOS TERMINARON ────────────────────────────
void ComprobarFin()
{
    if (consumidor1Terminado && consumidor2Terminado)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("  🏁 Ambos consumidores terminaron. Cerrando...");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        cts.Cancel();
    }
}

// ── INICIAR SENSOR ───────────────────────────────────────────
sensor.Iniciar();

Console.WriteLine("Presiona Ctrl+C para terminar.\n");
Console.WriteLine("═══════════════════════════════════════════════════════\n");

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (TaskCanceledException)
{
    // Terminación normal
}

Console.WriteLine("\nPrograma terminado.");

// ============================================================================
// Función auxiliar
// ============================================================================
static string FormatearMensaje(string consumidor, Notificacion notificacion, int count)
{
    var emoji = notificacion.Tipo switch
    {
        NotificacionTipo.Create => "🟢",
        NotificacionTipo.Update => "🟡",
        NotificacionTipo.Delete => "🔴",
        NotificacionTipo.Error   => "💥",
        _ => "⚪"
    };

    var timestamp = notificacion.Timestamp.ToString("HH:mm:ss.fff");

    if (notificacion.Tipo == NotificacionTipo.Error)
    {
        // Errores en rojo brillante con fondo
        return $"\u001b[31;1m[{timestamp}] 💥 {consumidor} ({count}/30) ⚠️  ERROR: {notificacion.Mensaje}\u001b[0m";
    }

    return $"[{timestamp}] {emoji} {consumidor} ({count}/30) {notificacion.Mensaje}";
}
