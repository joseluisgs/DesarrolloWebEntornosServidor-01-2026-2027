using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace _10_RepositorioRemoto.Notifications;

/// <summary>
/// Servicio de notificaciones que emite eventos de usuario mediante System.Reactive.
/// Usa un Subject&lt;UserEvent&gt; como emisor central de eventos.
/// </summary>
public class ConsoleNotificationService : INotificationService
{
    private readonly Subject<UserEvent> _subject = new();

    /// <inheritdoc />
    public IObservable<UserEvent> Events => _subject.AsObservable();

    /// <inheritdoc />
    public void NotifyUserCreated(int userId, string userName)
    {
        _subject.OnNext(new UserEvent(UserEventType.Created, userId, userName));
    }

    /// <inheritdoc />
    public void NotifyUserUpdated(int userId, string userName)
    {
        _subject.OnNext(new UserEvent(UserEventType.Updated, userId, userName));
    }

    /// <inheritdoc />
    public void NotifyUserDeleted(int userId)
    {
        _subject.OnNext(new UserEvent(UserEventType.Deleted, userId));
    }
}
