using FluentAssertions;
using NUnit.Framework;
using Testcontainers.MongoDb;
using MongoDB.Driver;
using _05_MongoDB_Driver_EFCore.Models;
using _05_MongoDB_Driver_EFCore.Repositories.MongoDriver;

namespace _05_MongoDB_Driver_EFCore.Tests;

/// <summary>
/// Tests de integración del repositorio MongoDB usando TestContainers.
/// Levanta un contenedor MongoDB real para verificar el driver nativo.
/// </summary>
[TestFixture]
public class ProductoMongoIntegrationTests
{
    private MongoDbContainer _container = null!;
    private ProductoMongoDriverRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = new MongoDbBuilder()
            .WithImage("mongo:7")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

        await _container.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _repository = new ProductoMongoDriverRepository(
            _container.GetConnectionString(), "test_productos_db");

        var client = new MongoClient(_container.GetConnectionString());
        var database = client.GetDatabase("test_productos_db");
        await database.DropCollectionAsync("productos");
    }

    [TestFixture]
    public class CasosValidos : ProductoMongoIntegrationTests
    {
        [Test]
        public async Task Create_ProductoValido_DeberiaCrearConId()
        {
            // Arrange
            var producto = new Producto
            {
                Nombre = "Teclado Mecánico",
                Precio = 79.99m,
                Categoria = "Periféricos"
            };

            // Act
            var resultado = await _repository.CreateAsync(producto);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().NotBeNullOrEmpty();
            resultado.Nombre.Should().Be("Teclado Mecánico");
            resultado.Precio.Should().Be(79.99m);
            resultado.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task GetAll_ConDatos_DeberiaRetornarTodos()
        {
            // Arrange
            await _repository.CreateAsync(new Producto
            {
                Nombre = "Producto A",
                Precio = 10m,
                Categoria = "Cat1"
            });
            await _repository.CreateAsync(new Producto
            {
                Nombre = "Producto B",
                Precio = 20m,
                Categoria = "Cat2"
            });

            // Act
            var resultados = (await _repository.GetAllAsync()).ToList();

            // Assert
            resultados.Should().HaveCount(2);
            resultados.Should().BeInAscendingOrder(p => p.Nombre);
        }

        [Test]
        public async Task GetById_Existente_DeberiaRetornarProducto()
        {
            // Arrange
            var creado = await _repository.CreateAsync(new Producto
            {
                Nombre = "Monitor Dell",
                Precio = 349.99m,
                Categoria = "Periféricos"
            });

            // Act
            var encontrado = await _repository.GetByIdAsync(creado.Id);

            // Assert
            encontrado.Should().NotBeNull();
            encontrado!.Nombre.Should().Be("Monitor Dell");
        }

        [Test]
        public async Task Update_Existente_DeberiaActualizar()
        {
            // Arrange
            var creado = await _repository.CreateAsync(new Producto
            {
                Nombre = "Producto",
                Precio = 10m,
                Categoria = "Cat"
            });
            var actualizado = new Producto
            {
                Id = creado.Id,
                Nombre = creado.Nombre,
                Precio = 15m,
                Categoria = creado.Categoria,
                CreatedAt = creado.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var resultado = await _repository.UpdateAsync(actualizado);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Precio.Should().Be(15m);
        }

        [Test]
        public async Task Delete_Existente_DeberiaEliminar()
        {
            // Arrange
            var creado = await _repository.CreateAsync(new Producto
            {
                Nombre = "Para Eliminar",
                Precio = 5m,
                Categoria = "Cat"
            });

            // Act
            var eliminado = await _repository.DeleteAsync(creado.Id);

            // Assert
            eliminado.Should().BeTrue();
            var verificacion = await _repository.GetByIdAsync(creado.Id);
            verificacion.Should().BeNull();
        }
    }

    [TestFixture]
    public class CasosNegativos : ProductoMongoIntegrationTests
    {
        [Test]
        public async Task GetById_Inexistente_DeberiaRetornarNull()
        {
            // Arrange & Act
            var resultado = await _repository.GetByIdAsync("507f1f77bcf86cd799439011");

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task Delete_Inexistente_DeberiaRetornarFalse()
        {
            // Arrange & Act
            var eliminado = await _repository.DeleteAsync("507f1f77bcf86cd799439011");

            // Assert
            eliminado.Should().BeFalse();
        }
    }
}
