using FluentAssertions;
using NUnit.Framework;
using _10_RepositorioRemoto.Dto;
using _10_RepositorioRemoto.Mappers;
using _10_RepositorioRemoto.Models;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests unitarios de UserMapper.
/// </summary>
[TestFixture]
public class UserMapperTests
{
    [TestFixture]
    public class ToResponse
    {
        [Test]
        public void ToResponse_UsuarioValido_RetornaDtoConTodosLosCampos()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Name = "Juan García",
                Username = "juangarcia",
                Email = "juan@email.com"
            };

            // Act
            var dto = user.ToResponse();

            // Assert
            dto.Should().NotBeNull();
            dto.Id.Should().Be(1);
            dto.Name.Should().Be("Juan García");
            dto.Username.Should().Be("juangarcia");
            dto.Email.Should().Be("juan@email.com");
        }

        [Test]
        public void ToResponse_ListaUsuarios_RetornaListaDto()
        {
            // Arrange
            var users = new List<User>
            {
                new() { Id = 1, Name = "Ana", Username = "ana", Email = "ana@test.com" },
                new() { Id = 2, Name = "Carlos", Username = "carlos", Email = "carlos@test.com" }
            };

            // Act
            var dtos = users.ToResponse();

            // Assert
            dtos.Should().HaveCount(2);
            dtos[0].Name.Should().Be("Ana");
            dtos[1].Name.Should().Be("Carlos");
        }

        [Test]
        public void ToResponse_ListaVacia_RetornaListaVacia()
        {
            // Arrange
            var users = new List<User>();

            // Act
            var dtos = users.ToResponse();

            // Assert
            dtos.Should().BeEmpty();
        }
    }

    [TestFixture]
    public class ToModelFromRequest
    {
        [Test]
        public void ToModel_CreateUserRequest_RetornaUserConIdCero()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Ana López",
                Username = "analopez",
                Email = "ana@email.com"
            };

            // Act
            var user = request.ToModel();

            // Assert
            user.Should().NotBeNull();
            user.Id.Should().Be(0);
            user.Name.Should().Be("Ana López");
            user.Username.Should().Be("analopez");
            user.Email.Should().Be("ana@email.com");
        }
    }

    [TestFixture]
    public class ToModelFromDto
    {
        [Test]
        public void ToModel_JsonPlaceholderUserDto_RetornaUserConId()
        {
            // Arrange
            var dto = new JsonPlaceholderUserDto
            {
                Id = 42,
                Name = "Carlos Ruiz",
                Username = "carlosruiz",
                Email = "carlos@email.com"
            };

            // Act
            var user = dto.ToModel();

            // Assert
            user.Should().NotBeNull();
            user.Id.Should().Be(42);
            user.Name.Should().Be("Carlos Ruiz");
            user.Username.Should().Be("carlosruiz");
            user.Email.Should().Be("carlos@email.com");
        }
    }
}
