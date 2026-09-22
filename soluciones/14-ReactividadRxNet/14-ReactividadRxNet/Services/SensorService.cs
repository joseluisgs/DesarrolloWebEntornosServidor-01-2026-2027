using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace _14_ReactividadRxNet.Services;

public enum NotificacionTipo
{
    Create,
    Update,
    Delete,
    Error
}

public record Notificacion(NotificacionTipo Tipo, string Mensaje, DateTime Timestamp);

/// <summary>
/// Sensor que produce notificaciones de forma continua (hot observable).
/// Los observadores solo ven eventos desde el momento en que se suscriben.
/// </summary>
public class SensorService : IDisposable
{
    private readonly Subject<Notificacion> _subject = new();
    private readonly Random _random = new();
    private Timer? _timer;
    private int _contador;

    /// <summary>
    /// Observable CALIENTE: si te conectas tarde, te pierdes lo que ya pasó.
    /// </summary>
    public IObservable<Notificacion> Observable => _subject.AsObservable();

    public void Iniciar()
    {
        Console.WriteLine("Sensor iniciado. Produciendo notificaciones...\n");
        _timer = new Timer(ProducirNotificacion, null, 0, _random.Next(1000, 5000));
    }

    private void ProducirNotificacion(object? state)
    {
        _contador++;

        var tipo = _random.Next(100) < 20
            ? NotificacionTipo.Error
            : _random.Next(3) switch
            {
                0 => NotificacionTipo.Create,
                1 => NotificacionTipo.Update,
                _ => NotificacionTipo.Delete
            };

        var mensaje = tipo switch
        {
            NotificacionTipo.Create => $"Producto #{_contador} creado",
            NotificacionTipo.Update => $"Producto #{_contador} actualizado",
            NotificacionTipo.Delete => $"Producto #{_contador} eliminado",
            NotificacionTipo.Error => $"Error al procesar producto #{_contador}",
            _ => $"Notificación #{_contador}"
        };

        // OnNext: emite a TODOS los suscriptores activos en este momento
        // Si un observador NO está suscrito, NO recibe este evento
        _subject.OnNext(new Notificacion(tipo, mensaje, DateTime.Now));

        _timer?.Change(_random.Next(1000, 5000), Timeout.Infinite);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _subject.OnCompleted();
        _subject.Dispose();
    }
}
