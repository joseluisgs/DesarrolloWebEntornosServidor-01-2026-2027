using FluentAssertions;
using NUnit.Framework;
using _10_RepositorioRemoto.Dto;
using _10_RepositorioRemoto.Errors;
using _10_RepositorioRemoto.Validators;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests unitarios de CreateUserRequestValidator.
/// </summary>
[TestFixture]
public class CreateUserRequestValidatorTests
{
    [TestFixture]
    public class CasosValidos
    {
        [Test]
        public void Validate_DatosCorrectos_RetornaSuccess()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan García",
                Username = "juangarcia",
                Email = "juan@email.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
            resultado.Value.Should().Be(request);
        }

        [Test]
        public void Validate_NombreMinimo_RetornaSuccess()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Ana",
                Username = "ana",
                Email = "ana@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
        }

        [Test]
        public void Validate_NombreMaximo_RetornaSuccess()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = new string('A', 100),
                Username = "usuario",
                Email = "test@email.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsSuccess.Should().BeTrue();
        }
    }

    [TestFixture]
    public class CasosInvalidos
    {
        [Test]
        public void Validate_NombreNulo_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = null!,
                Username = "juan",
                Email = "juan@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            resultado.Error.Should().BeOfType<DomainError.ValidationError>();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Name");
            error.Message.Should().Contain("obligatorio");
        }

        [Test]
        public void Validate_NombreVacio_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "",
                Username = "juan",
                Email = "juan@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Name");
        }

        [Test]
        public void Validate_NombreSoloEspacios_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "   ",
                Username = "juan",
                Email = "juan@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Name");
        }

        [Test]
        public void Validate_NombreDemasiadoCorto_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "A",
                Username = "juan",
                Email = "juan@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Name");
            error.Message.Should().Contain("2 y 100");
        }

        [Test]
        public void Validate_NombreDemasiadoLargo_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = new string('A', 101),
                Username = "juan",
                Email = "juan@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Name");
        }

        [Test]
        public void Validate_UsernameVacio_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "",
                Email = "juan@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Username");
        }

        [Test]
        public void Validate_UsernameDemasiadoCorto_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "ab",
                Email = "juan@test.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Username");
            error.Message.Should().Contain("3 y 50");
        }

        [Test]
        public void Validate_EmailVacio_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = ""
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Email");
        }

        [Test]
        public void Validate_EmailSinArroba_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = "juanemail.com"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Email");
            error.Message.Should().Contain("formato válido");
        }

        [Test]
        public void Validate_EmailSinPunto_RetornaValidationError()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "Juan",
                Username = "juan",
                Email = "juan@emailcom"
            };

            // Act
            var resultado = CreateUserRequestValidator.Validate(request);

            // Assert
            resultado.IsFailure.Should().BeTrue();
            var error = (DomainError.ValidationError)resultado.Error;
            error.Field.Should().Be("Email");
        }
    }
}
