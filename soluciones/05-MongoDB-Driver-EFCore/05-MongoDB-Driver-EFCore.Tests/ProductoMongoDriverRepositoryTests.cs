using FluentAssertions;
using Moq;
using NUnit.Framework;
using MongoDB.Driver;
using _05_MongoDB_Driver_EFCore.Models;
using _05_MongoDB_Driver_EFCore.Repositories.Base;

namespace _05_MongoDB_Driver_EFCore.Tests;

/// <summary>
/// Tests unitarios del repositorio de productos MongoDB.
/// Usa Moq para simular el driver de MongoDB.
/// </summary>
[TestFixture]
public class ProductoMongoDriverRepositoryTests
{
    private Mock<IMongoCollection<Producto>> _mockCollection = null!;
    private Mock<IMongoDatabase> _mockDatabase = null!;
    private Mock<IMongoClient> _mockClient = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCollection = new Mock<IMongoCollection<Producto>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockClient = new Mock<IMongoClient>();

        _mockDatabase
            .Setup(d => d.GetCollection<Producto>("productos", null))
            .Returns(_mockCollection.Object);

        _mockClient
            .Setup(c => c.GetDatabase("productos_db", null))
            .Returns(_mockDatabase.Object);
    }

    [TestFixture]
    public class CasosValidos : ProductoMongoDriverRepositoryTests
    {
        [Test]
        public async Task CreateAsync_ProductoValido_RetornaProductoConId()
        {
            // Arrange
            var producto = new Producto
            {
                Nombre = "Teclado",
                Precio = 79.99m,
                Categoria = "Periféricos"
            };

            _mockCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<Producto>(), It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var repository = new Repositories.MongoDriver.ProductoMongoDriverRepository(
                _mockClient.Object, "productos_db");
            var resultado = await repository.CreateAsync(producto);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Nombre.Should().Be("Teclado");
            resultado.Precio.Should().Be(79.99m);
            resultado.Id.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task DeleteAsync_ProductoExistente_RetornaTrue()
        {
            // Arrange
            var deleteResult = new DeleteResult.Acknowledged(1);
            _mockCollection
                .Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<Producto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult);

            // Act
            var repository = new Repositories.MongoDriver.ProductoMongoDriverRepository(
                _mockClient.Object, "productos_db");
            var resultado = await repository.DeleteAsync("id_valido");

            // Assert
            resultado.Should().BeTrue();
        }
    }

    [TestFixture]
    public class CasosNegativos : ProductoMongoDriverRepositoryTests
    {
        [Test]
        public async Task DeleteAsync_ProductoInexistente_RetornaFalse()
        {
            // Arrange
            var deleteResult = new DeleteResult.Acknowledged(0);
            _mockCollection
                .Setup(c => c.DeleteOneAsync(It.IsAny<FilterDefinition<Producto>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleteResult);

            // Act
            var repository = new Repositories.MongoDriver.ProductoMongoDriverRepository(
                _mockClient.Object, "productos_db");
            var resultado = await repository.DeleteAsync("id_inexistente");

            // Assert
            resultado.Should().BeFalse();
        }
    }
}
