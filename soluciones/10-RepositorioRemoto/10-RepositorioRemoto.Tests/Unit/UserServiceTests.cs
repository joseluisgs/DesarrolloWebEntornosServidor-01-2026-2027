using FluentAssertions;
using Moq;
using NUnit.Framework;
using _10_RepositorioRemoto.Api;
using _10_RepositorioRemoto.Cache;
using _10_RepositorioRemoto.Dto;
using _10_RepositorioRemoto.Errors;
using _10_RepositorioRemoto.Models;
using _10_RepositorioRemoto.Notifications;
using _10_RepositorioRemoto.Repositories;
using _10_RepositorioRemoto.Services;
using Microsoft.Extensions.Logging;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests unitarios de UserService con Moq para dependencias externas.
/// </summary>
[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _mockRepository = null!;
    private Mock<ICacheService> _mockCache = null!;
    private Mock<IJsonPlaceholderApi> _mockApi = null!;
    private Mock<INotificationService> _mockNotifications = null!;
    private Mock<ILogger<UserService>> _mockLogger = null!;
    private UserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockCache = new Mock<ICacheService>();
        _mockApi = new Mock<IJsonPlaceholderApi>();
        _mockNotifications = new Mock<INotificationService>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _service = new UserService(
            _mockRepository.Object,
            _mockCache.Object,
            _mockApi.Object,
            _mockNotifications.Object,
            _mockLogger.Object);
    }

    [TestFixture]
    public class CasosPositivos : UserServiceTests
    {
        [Test]
        public async Task GetAllAsync_ConDatos_RetornaTodosLosUsuarios()
        {
            // Arrange
            var usuarios = new List<User>
            {
                new() { Id = 1, Name = "Juan", Username = "juan", Email = "juan@test.com" },
                new() { Id = 2, Name = "Ana", Username = "ana", Email = "ana@test.com" }
            };
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(usuarios);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(2);
            resultado[0].Name.Should().Be("Juan");
            resultado[1].Name.Should().Be("Ana");
        }

        [Test]
        public async Task GetAllAsync_ConCacheHit_RetornaDelCache()
        {
            // Arrange
            var usuarios = new List<User>
            {
                new() { Id = 1, Name = "Juan", Username = "juan", Email = "juan@test.com" }
            };
            _mockCache.Setup(c => c.GetAsync<IReadOnlyList<User>>("users:all"))
                .ReturnsAsync(usuarios);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(1);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Never);
        }

        [Test]
        public async Task GetAllAsync_BDVacia_SincronizaDesdeRemoto()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var remoteUsers = new List<JsonPlaceholderUserDto>
            {
                new() { Id = 1, Name = "Remote User", Username = "remote", Email = "remote@test.com" }
            };
            _mockApi.Setup(a => a.GetUsersAsync()).ReturnsAsync(remoteUsers);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            _mockApi.Verify(a => a.GetUsersAsync(), Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_Existente_DeberiaRetornarUsuario()
        {
            // Arrange
            var usuario = new User { Id = 1, Name = "Juan", Username = "juan", Email = "juan@test.com" };
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(usuario);

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Name.Should().Be("Juan");
        }

        [Test]
        public async Task GetByIdAsync_ConCacheHit_RetornaDelCache()
        {
            // Arrange
            var usuario = new User { Id = 1, Name = "Juan", Username = "juan", Email = "juan@test.com" };
            _mockCache.Setup(c => c.GetAsync<User>("users:1")).ReturnsAsync(usuario);

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Name.Should().Be("Juan");
            _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_NoEnLocal_BuscaEnApiRemota()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((User?)null);

            var remoteDto = new JsonPlaceholderUserDto
            {
                Id = 42,
                Name = "API User",
                Username = "apiuser",
                Email = "api@test.com"
            };
            _mockApi.Setup(a => a.GetUserByIdAsync(42)).ReturnsAsync(remoteDto);
            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(new User { Id = 42, Name = "API User", Username = "apiuser", Email = "api@test.com" });

            // Act
            var resultado = await _service.GetByIdAsync(42);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Name.Should().Be("API User");
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task CreateAsync_DatosValidos_DeberiaCrearUsuario()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = "juan@test.com"
            };

            var dto = new JsonPlaceholderUserDto
            {
                Id = 1,
                Name = "Juan",
                Username = "juan",
                Email = "juan@test.com"
            };

            _mockApi.Setup(a => a.CreateUserAsync(It.IsAny<JsonPlaceholderUserDto>()))
                .ReturnsAsync(dto);

            _mockRepository.Setup(r => r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(new User { Id = 1, Name = "Juan", Username = "juan", Email = "juan@test.com" });

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Name.Should().Be("Juan");
            _mockNotifications.Verify(n => n.NotifyUserCreated(1, "Juan"), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_DatosValidos_DeberiaActualizarUsuario()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan García",
                Username = "juangarcia",
                Email = "juan@email.com"
            };

            _mockApi.Setup(a => a.UpdateUserAsync(1, It.IsAny<JsonPlaceholderUserDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Name.Should().Be("Juan García");
            _mockNotifications.Verify(n => n.NotifyUserUpdated(1, "Juan García"), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_Existente_DeberiaEliminar()
        {
            // Arrange
            _mockApi.Setup(a => a.DeleteUserAsync(1)).Returns(Task.CompletedTask);
            _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var resultado = await _service.DeleteAsync(1);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().BeTrue();
            _mockNotifications.Verify(n => n.NotifyUserDeleted(1), Times.Once);
        }

        [Test]
        public async Task ExportToJsonAsync_ConDatos_GeneraFichero()
        {
            // Arrange
            var usuarios = new List<User>
            {
                new() { Id = 1, Name = "Juan", Username = "juan", Email = "juan@test.com" }
            };
            _mockCache.Setup(c => c.GetAsync<IReadOnlyList<User>>("users:all"))
                .ReturnsAsync((IReadOnlyList<User>?)null);
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(usuarios);

            // Act
            var path = await _service.ExportToJsonAsync();

            // Assert
            path.Should().NotBeNullOrEmpty();
            File.Exists(path).Should().BeTrue();
        }

        [Test]
        public async Task SyncFromRemoteAsync_ConDatos_SincronizaBD()
        {
            // Arrange
            var remoteUsers = new List<JsonPlaceholderUserDto>
            {
                new() { Id = 1, Name = "Remote", Username = "remote", Email = "remote@test.com" }
            };
            _mockApi.Setup(a => a.GetUsersAsync()).ReturnsAsync(remoteUsers);

            // Act
            await _service.SyncFromRemoteAsync();

            // Assert
            _mockRepository.Verify(r => r.DeleteAllAsync(), Times.Once);
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        }
    }

    [TestFixture]
    public class CasosNegativos : UserServiceTests
    {
        [Test]
        public async Task CreateAsync_NombreVacio_DeberiaFallar()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "",
                Username = "juan",
                Email = "juan@test.com"
            };

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ValidationError>();
            ((DomainError.ValidationError)resultado.Error).Field.Should().Be("Name");
        }

        [Test]
        public async Task CreateAsync_UsernameVacio_DeberiaFallar()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "",
                Email = "juan@test.com"
            };

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ValidationError>();
            ((DomainError.ValidationError)resultado.Error).Field.Should().Be("Username");
        }

        [Test]
        public async Task CreateAsync_EmailVacio_DeberiaFallar()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = ""
            };

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ValidationError>();
            ((DomainError.ValidationError)resultado.Error).Field.Should().Be("Email");
        }

        [Test]
        public async Task CreateAsync_ApiFalla_DeberiaFallar()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = "juan@test.com"
            };

            _mockApi.Setup(a => a.CreateUserAsync(It.IsAny<JsonPlaceholderUserDto>()))
                .ThrowsAsync(new Exception("API down"));

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ApiError>();
        }

        [Test]
        public async Task UpdateAsync_NombreVacio_DeberiaFallar()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "",
                Username = "juan",
                Email = "juan@test.com"
            };

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ValidationError>();
            ((DomainError.ValidationError)resultado.Error).Field.Should().Be("Name");
        }

        [Test]
        public async Task UpdateAsync_EmailInvalido_DeberiaFallar()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = "no-email"
            };

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ValidationError>();
            ((DomainError.ValidationError)resultado.Error).Field.Should().Be("Email");
        }

        [Test]
        public async Task UpdateAsync_ApiFalla_DeberiaFallar()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = "juan@test.com"
            };

            _mockApi.Setup(a => a.UpdateUserAsync(1, It.IsAny<JsonPlaceholderUserDto>()))
                .ThrowsAsync(new Exception("API down"));

            // Act
            var resultado = await _service.UpdateAsync(1, request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
        }

        [Test]
        public async Task DeleteAsync_ApiFalla_DeberiaFallar()
        {
            // Arrange
            _mockApi.Setup(a => a.DeleteUserAsync(1))
                .ThrowsAsync(new Exception("API down"));

            // Act
            var resultado = await _service.DeleteAsync(1);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.NotFound>();
        }

        [Test]
        public async Task GetByIdAsync_Inexistente_DeberiaFallar()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);
            _mockApi.Setup(a => a.GetUserByIdAsync(999))
                .ThrowsAsync(new Exception("Not found"));

            // Act
            var resultado = await _service.GetByIdAsync(999);

            // Assert
            resultado.IsFailure.Should().BeTrue();
        }

        [Test]
        public async Task SyncFromRemoteAsync_ApiFalla_NoPropagaExcepcion()
        {
            // Arrange
            _mockApi.Setup(a => a.GetUsersAsync())
                .ThrowsAsync(new Exception("API down"));

            // Act & Assert
            await _service.Invoking(s => s.SyncFromRemoteAsync())
                .Should().NotThrowAsync();
        }
    }
}
