using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using _08_TestContainers.Entity;
using _08_TestContainers.Models;
using _08_TestContainers.Repositories;

namespace _08_TestContainers.Tests;

/// <summary>
/// Tests de integración de ProductoRepository usando TestContainers.
/// Cada test crea un contenedor PostgreSQL efímero, ejecuta las pruebas
/// y lo destruye automáticamente. No depende de infraestructura externa.
/// </summary>
[TestFixture]
public class ProductoRepositoryTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private ProductoRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Crear un contenedor PostgreSQL que se reutiliza en todos los tests
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("test")
            .WithPassword("test")
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
        // Crear un contexto fresco para cada test (aislamiento)
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(_container.GetConnectionString());

        _context = new AppDbContext(optionsBuilder.Options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new ProductoRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        // Limpiar la base de datos después de cada test
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Test]
    public async Task Create_InsertsProduct()
    {
        // Arrange
        var producto = new Producto(0, "Teclado Mecánico", 79.99m, "Periféricos");

        // Act
        var resultado = await _repository.CreateAsync(producto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().BeGreaterThan(0);
        resultado.Nombre.Should().Be("Teclado Mecánico");
        resultado.Precio.Should().Be(79.99m);
    }

    [Test]
    public async Task GetAll_ReturnsAllProducts()
    {
        // Arrange
        await _repository.CreateAsync(new Producto(0, "Producto A", 10m, "Cat1"));
        await _repository.CreateAsync(new Producto(0, "Producto B", 20m, "Cat2"));
        await _repository.CreateAsync(new Producto(0, "Producto C", 30m, "Cat1"));

        // Act
        var resultados = await _repository.GetAllAsync();

        // Assert
        resultados.Should().HaveCount(3);
        resultados.Should().BeInAscendingOrder(p => p.Nombre);
    }

    [Test]
    public async Task GetById_ExistingProduct_ReturnsProduct()
    {
        // Arrange
        var creado = await _repository.CreateAsync(
            new Producto(0, "Monitor Dell", 349.99m, "Periféricos"));

        // Act
        var encontrado = await _repository.GetByIdAsync(creado.Id);

        // Assert
        encontrado.Should().NotBeNull();
        encontrado!.Id.Should().Be(creado.Id);
        encontrado.Nombre.Should().Be("Monitor Dell");
    }

    [Test]
    public async Task GetById_NonExistingProduct_ReturnsNull()
    {
        // Arrange
        var idInexistente = 9999;

        // Act
        var resultado = await _repository.GetByIdAsync(idInexistente);

        // Assert
        resultado.Should().BeNull();
    }

    [Test]
    public async Task Delete_ExistingProduct_RemovesProduct()
    {
        // Arrange
        var creado = await _repository.CreateAsync(
            new Producto(0, "Ratón Bluetooth", 29.99m, "Periféricos"));

        // Act
        var eliminado = await _repository.DeleteAsync(creado.Id);

        // Assert
        eliminado.Should().BeTrue();

        // Verificar que ya no existe
        var verificacion = await _repository.GetByIdAsync(creado.Id);
        verificacion.Should().BeNull();
    }
}
