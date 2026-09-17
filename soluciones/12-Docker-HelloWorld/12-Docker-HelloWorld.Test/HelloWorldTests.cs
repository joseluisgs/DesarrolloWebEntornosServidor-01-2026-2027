using FluentAssertions;
using NUnit.Framework;

namespace _12_Docker_HelloWorld.Test;

/// <summary>
/// Tests del ejemplo Docker Hello World.
/// Si alguno falla, el Dockerfile NO compila la imagen.
/// </summary>
[TestFixture]
public class HelloWorldTests
{
    [Test]
    public void SanityCheck_TrueEsTrue()
    {
        // Arrange
        bool esVerdad = true;

        // Act
        // (no hay acción, es una comprobación estática)

        // Assert
        esVerdad.Should().BeTrue();
    }

    [Test]
    public void MensajeConsola_NoEsNulo()
    {
        // Arrange
        string mensaje = "Hola desde Docker";

        // Assert
        mensaje.Should().NotBeNullOrEmpty();
        mensaje.Should().Contain("Docker");
    }

    [Test]
    public void RutaHealth_EsValida()
    {
        // Arrange
        string ruta = "/health";

        // Assert
        ruta.Should().StartWith("/");
        ruta.Should().Be("/health");
    }
}
