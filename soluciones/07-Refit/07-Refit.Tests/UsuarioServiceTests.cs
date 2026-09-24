using FluentAssertions;
using Moq;
using NUnit.Framework;
using Refit;
using _07_Refit.Api;
using _07_Refit.Dto;
using _07_Refit.Errors;
using _07_Refit.Models;
using _07_Refit.Services;

namespace _07_Refit.Tests;

/// <summary>
/// Tests unitarios de UsuarioService con Moq para IJsonPlaceholderApi.
/// </summary>
[TestFixture]
public class UsuarioServiceTests
{
    private Mock<IJsonPlaceholderApi> _mockApi = null!;
    private UsuarioService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockApi = new Mock<IJsonPlaceholderApi>();
        _service = new UsuarioService(_mockApi.Object);
    }

    [TestFixture]
    public class CasosPositivos : UsuarioServiceTests
    {
        [Test]
        public async Task GetAllAsync_DeberiaRetornarTodosLosUsuarios()
        {
            // Arrange
            var usuarios = new List<Usuario>
            {
                new(1, "Juan", "juan", "juan@test.com"),
                new(2, "Ana", "ana", "ana@test.com")
            };
            _mockApi.Setup(a => a.GetUsuariosAsync()).ReturnsAsync(usuarios);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(2);
            resultado[0].Name.Should().Be("Juan");
        }

        [Test]
        public async Task GetByIdAsync_Existente_DeberiaRetornarSuccess()
        {
            // Arrange
            var usuario = new Usuario(1, "Juan", "juan", "juan@test.com");
            _mockApi.Setup(a => a.GetUsuarioByIdAsync(1)).ReturnsAsync(usuario);

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.Should().BeOfType<Result<Usuario, DomainError>.Success>();
            if (resultado is Result<Usuario, DomainError>.Success success)
            {
                success.Value.Name.Should().Be("Juan");
            }
        }

        [Test]
        public async Task CreateAsync_DatosValidos_DeberiaRetornarSuccess()
        {
            // Arrange
            var request = new CreateUserRequest("Juan", "juan", "juan@test.com");
            var creado = new Usuario(1, "Juan", "juan", "juan@test.com");
            _mockApi.Setup(a => a.CreateUsuarioAsync(request)).ReturnsAsync(creado);

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.Should().BeOfType<Result<Usuario, DomainError>.Success>();
            if (resultado is Result<Usuario, DomainError>.Success success)
            {
                success.Value.Id.Should().Be(1);
            }
        }

        [Test]
        public async Task DeleteAsync_Existente_DeberiaRetornarSuccess()
        {
            // Arrange
            _mockApi.Setup(a => a.DeleteUsuarioAsync(1)).Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.DeleteAsync(1);

            // Assert
            resultado.Should().BeOfType<Result<bool, DomainError>.Success>();
        }
    }

    [TestFixture]
    public class CasosNegativos : UsuarioServiceTests
    {
        [Test]
        public async Task GetByIdAsync_Inexistente_DeberiaRetornarFailure()
        {
            // Arrange
            _mockApi.Setup(a => a.GetUsuarioByIdAsync(999))
                .ReturnsAsync((Usuario?)null);

            // Act
            var resultado = await _service.GetByIdAsync(999);

            // Assert
            resultado.Should().BeOfType<Result<Usuario, DomainError>.Failure>();
            if (resultado is Result<Usuario, DomainError>.Failure failure)
            {
                failure.Error.Should().BeOfType<DomainError.NotFound>();
            }
        }

        [Test]
        public async Task CreateAsync_NombreVacio_DeberiaRetornarValidationError()
        {
            // Arrange
            var request = new CreateUserRequest("", "juan", "juan@test.com");

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.Should().BeOfType<Result<Usuario, DomainError>.Failure>();
            if (resultado is Result<Usuario, DomainError>.Failure failure)
            {
                failure.Error.Should().BeOfType<DomainError.ValidationError>();
            }
        }

        [Test]
        public async Task CreateAsync_EmailVacio_DeberiaRetornarValidationError()
        {
            // Arrange
            var request = new CreateUserRequest("Juan", "juan", "");

            // Act
            var resultado = await _service.CreateAsync(request);

            // Assert
            resultado.Should().BeOfType<Result<Usuario, DomainError>.Failure>();
            if (resultado is Result<Usuario, DomainError>.Failure failure)
            {
                failure.Error.Should().BeOfType<DomainError.ValidationError>();
            }
        }
    }
}
