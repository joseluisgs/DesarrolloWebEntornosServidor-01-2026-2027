using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using _04_PostgreSQL_Dapper_EFCore.Entity;
using _04_PostgreSQL_Dapper_EFCore.Models;
using _04_PostgreSQL_Dapper_EFCore.Repositories.Dapper;

namespace _04_PostgreSQL_Dapper_EFCore.Tests;

/// <summary>
/// Tests de integración de ProductoDapperRepository usando TestContainers.
/// Verifica que Dapper funciona correctamente contra PostgreSQL real.
/// </summary>
[TestFixture]
public class ProductoDapperRepositoryTests
{
    private PostgreSqlContainer _container = null!;
    private ProductoDapperRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();

        // Crear la tabla de productos en el contenedor
        using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options);
        await context.Database.EnsureCreatedAsync();
        await context.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        _repository = new ProductoDapperRepository(_container.GetConnectionString());

        using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options);
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE Productos RESTART IDENTITY");
    }

    [TestFixture]
    public class CasosPositivos : ProductoDapperRepositoryTests
    {
        [Test]
        public async Task Create_ProductoValido_DeberiaCrearConId()
        {
            // Arrange
            var producto = new Producto(0, "Teclado Mecánico", 79.99m, "Periféricos");

            // Act
            var resultado = await _repository.CreateAsync(producto);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().BeGreaterThan(0);
            resultado.Nombre.Should().Be("Teclado Mecánico");
        }

        [Test]
        public async Task GetAll_ConDatos_DeberiaRetornarTodos()
        {
            // Arrange
            await _repository.CreateAsync(new Producto(0, "Producto A", 10m, "Cat1"));
            await _repository.CreateAsync(new Producto(0, "Producto B", 20m, "Cat2"));

            // Act
            var resultados = await _repository.GetAllAsync();

            // Assert
            resultados.Should().HaveCount(2);
        }

        [Test]
        public async Task GetById_Existente_DeberiaRetornarProducto()
        {
            // Arrange
            var creado = await _repository.CreateAsync(
                new Producto(0, "Monitor Dell", 349.99m, "Periféricos"));

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
            var creado = await _repository.CreateAsync(
                new Producto(0, "Producto", 10m, "Cat"));
            var actualizado = creado with { Precio = 15m };

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
            var creado = await _repository.CreateAsync(
                new Producto(0, "Para Eliminar", 5m, "Cat"));

            // Act
            var eliminado = await _repository.DeleteAsync(creado.Id);

            // Assert
            eliminado.Should().BeTrue();
            var verificacion = await _repository.GetByIdAsync(creado.Id);
            verificacion.Should().BeNull();
        }
    }

    [TestFixture]
    public class CasosNegativos : ProductoDapperRepositoryTests
    {
        [Test]
        public async Task GetById_Inexistente_DeberiaRetornarNull()
        {
            // Arrange & Act
            var resultado = await _repository.GetByIdAsync(9999);

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task Update_Inexistente_DeberiaRetornarNull()
        {
            // Arrange
            var producto = new Producto(9999, "No existe", 10m, "Cat");

            // Act
            var resultado = await _repository.UpdateAsync(producto);

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task Delete_Inexistente_DeberiaRetornarFalse()
        {
            // Arrange & Act
            var eliminado = await _repository.DeleteAsync(9999);

            // Assert
            eliminado.Should().BeFalse();
        }
    }
}
