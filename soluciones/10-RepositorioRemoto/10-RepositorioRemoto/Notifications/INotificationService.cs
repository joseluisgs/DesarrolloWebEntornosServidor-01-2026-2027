using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace _10_RepositorioRemoto.Notifications;

/// <summary>
/// Interfaz para el servicio de notificaciones de eventos de usuarios.
/// Usa System.Reactive (Rx.NET) con Subject&lt;T&gt; para notificaciones reactivas.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Observable que emite eventos de usuario.
    /// </summary>
    IObservable<UserEvent> Events { get; }

    /// <summary>
    /// Emite un evento de usuario creado.
    /// </summary>
    void NotifyUserCreated(int userId, string userName);

    /// <summary>
    /// Emite un evento de usuario actualizado.
    /// </summary>
    void NotifyUserUpdated(int userId, string userName);

    /// <summary>
    /// Emite un evento de usuario eliminado.
    /// </summary>
    void NotifyUserDeleted(int userId);
}

/// <summary>
/// Evento de usuario con tipo de operación.
/// </summary>
public record UserEvent(UserEventType Type, int UserId, string? UserName = null);

/// <summary>
/// Tipos de operación de usuario.
/// </summary>
public enum UserEventType
{
    Created,
    Updated,
    Deleted
}
