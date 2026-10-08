using System.Reactive.Linq;
using FluentAssertions;
using NUnit.Framework;
using _10_RepositorioRemoto.Notifications;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests del servicio de notificaciones con System.Reactive (Rx.NET).
/// </summary>
[TestFixture]
public class NotificationServiceTests
{
    private ConsoleNotificationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ConsoleNotificationService();
    }

    [TestFixture]
    public class CasosPositivos : NotificationServiceTests
    {
        [Test]
        public void NotifyUserCreated_EmiteEvento_DeberiaEmitirUserEvent()
        {
            // Arrange
            UserEvent? eventoRecibido = null;
            _service.Events.Subscribe(evt => eventoRecibido = evt);

            // Act
            _service.NotifyUserCreated(1, "Juan");

            // Assert
            eventoRecibido.Should().NotBeNull();
            eventoRecibido!.Type.Should().Be(UserEventType.Created);
            eventoRecibido.UserId.Should().Be(1);
            eventoRecibido.UserName.Should().Be("Juan");
        }

        [Test]
        public void NotifyUserUpdated_EmiteEvento_DeberiaEmitirUserEvent()
        {
            // Arrange
            UserEvent? eventoRecibido = null;
            _service.Events.Subscribe(evt => eventoRecibido = evt);

            // Act
            _service.NotifyUserUpdated(2, "Ana");

            // Assert
            eventoRecibido.Should().NotBeNull();
            eventoRecibido!.Type.Should().Be(UserEventType.Updated);
            eventoRecibido.UserId.Should().Be(2);
            eventoRecibido.UserName.Should().Be("Ana");
        }

        [Test]
        public void NotifyUserDeleted_EmiteEvento_DeberiaEmitirUserEvent()
        {
            // Arrange
            UserEvent? eventoRecibido = null;
            _service.Events.Subscribe(evt => eventoRecibido = evt);

            // Act
            _service.NotifyUserDeleted(3);

            // Assert
            eventoRecibido.Should().NotBeNull();
            eventoRecibido!.Type.Should().Be(UserEventType.Deleted);
            eventoRecibido.UserId.Should().Be(3);
            eventoRecibido.UserName.Should().BeNull();
        }

        [Test]
        public void MultiplesEventos_DeberiaRecibirTodos()
        {
            // Arrange
            var eventos = new List<UserEvent>();
            _service.Events.Subscribe(evt => eventos.Add(evt));

            // Act
            _service.NotifyUserCreated(1, "Juan");
            _service.NotifyUserUpdated(2, "Ana");
            _service.NotifyUserDeleted(3);

            // Assert
            eventos.Should().HaveCount(3);
            eventos[0].Type.Should().Be(UserEventType.Created);
            eventos[1].Type.Should().Be(UserEventType.Updated);
            eventos[2].Type.Should().Be(UserEventType.Deleted);
        }

        [Test]
        public void Subscribe_DespuesDeEmitir_NoRecibeEventosAnteriores()
        {
            // Arrange
            _service.NotifyUserCreated(1, "Juan");
            UserEvent? eventoRecibido = null;

            // Act - Suscribirse después de emitir
            _service.Events.Subscribe(evt => eventoRecibido = evt);

            // Assert - No debería recibir el evento anterior
            eventoRecibido.Should().BeNull();
        }
    }

    [TestFixture]
    public class CasosNegativos : NotificationServiceTests
    {
        [Test]
        public void SinSuscriptores_NoLanzaExcepcion()
        {
            // Act & Assert - No debería lanzar excepción
            var accion = () =>
            {
                _service.NotifyUserCreated(1, "Juan");
                _service.NotifyUserUpdated(2, "Ana");
                _service.NotifyUserDeleted(3);
            };

            accion.Should().NotThrow();
        }
    }
}
