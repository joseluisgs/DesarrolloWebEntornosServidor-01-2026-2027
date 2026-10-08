using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using _10_RepositorioRemoto.Config;
using _10_RepositorioRemoto.Entity;
using _10_RepositorioRemoto.Models;
using _10_RepositorioRemoto.Repositories;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests de integración de UserRepository contra <b>PostgreSQL real</b>
/// usando Testcontainers: un contenedor efímero que se levanta en OneTimeSetUp
/// y se destruye en OneTimeTearDown. No depende de infraestructura instalada.
/// Verifica que la misma implementación funciona en el proveedor de PRODUCCIÓN.
/// </summary>
[TestFixture]
[Category("Integration")]
public class UserRepositoryPostgresTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private UserRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Contenedor PostgreSQL efímero reutilizado por todos los tests
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("usuarios_test")
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
        // Contexto fresco por test: mismo patrón que DependenciesProvider
        var infra = new InfraSettings
        {
            Database = DatabaseProviders.PostgreSql,
            ConnectionStrings = new ConnectionStrings
            {
                PostgreSql = _container.GetConnectionString()
            }
        };

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(infra.ConnectionStrings.PostgreSql)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new UserRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        // BD limpia entre tests para no acumular datos
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Test]
    public async Task CreateAsync_UsuarioValido_SeGuardaEnPostgreSQL()
    {
        // Arrange
        var user = new User { Id = 0, Name = "Ana García", Username = "anagarcia", Email = "ana@email.com" };

        // Act
        var result = await _repository.CreateAsync(user);
        var all = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Ana García");
        all.Should().HaveCount(1);
        all[0].Name.Should().Be("Ana García");
    }

    [Test]
    public async Task GetAllAsync_ConDatos_RetornaTodos()
    {
        // Arrange
        await _repository.CreateAsync(new User { Id = 0, Name = "Ana", Username = "ana", Email = "ana@test.com" });
        await _repository.CreateAsync(new User { Id = 0, Name = "Bob", Username = "bob", Email = "bob@test.com" });

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task GetByIdAsync_Existente_RetornaUsuario()
    {
        // Arrange
        await _repository.CreateAsync(new User { Id = 0, Name = "Ana", Username = "ana", Email = "ana@test.com" });
        var created = (await _repository.GetAllAsync()).First();

        // Act
        var result = await _repository.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Ana");
    }

    [Test]
    public async Task GetByIdAsync_Inexistente_RetornaNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateAsync_Existente_RetornaUsuarioActualizado()
    {
        // Arrange
        await _repository.CreateAsync(new User { Id = 0, Name = "Ana", Username = "ana", Email = "ana@test.com" });
        var created = (await _repository.GetAllAsync()).First();

        // Act
        var result = await _repository.UpdateAsync(new User
        {
            Id = created.Id,
            Name = "Ana García",
            Username = "anagarcia",
            Email = "ana@email.com"
        });

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Ana García");
    }

    [Test]
    public async Task DeleteAsync_Existente_RetornaTrueYBorra()
    {
        // Arrange
        await _repository.CreateAsync(new User { Id = 0, Name = "Ana", Username = "ana", Email = "ana@test.com" });
        var created = (await _repository.GetAllAsync()).First();

        // Act
        var result = await _repository.DeleteAsync(created.Id);

        // Assert
        result.Should().BeTrue();
        (await _repository.GetAllAsync()).Should().BeEmpty();
    }

    [Test]
    public async Task DeleteAllAsync_ConDatos_EliminaTodos()
    {
        // Arrange
        await _repository.CreateAsync(new User { Id = 0, Name = "A", Username = "a", Email = "a@test.com" });
        await _repository.CreateAsync(new User { Id = 0, Name = "B", Username = "b", Email = "b@test.com" });

        // Act
        await _repository.DeleteAllAsync();

        // Assert
        (await _repository.CountAsync()).Should().Be(0);
    }

    [Test]
    public async Task CountAsync_ConDatos_RetornaCantidad()
    {
        // Arrange
        await _repository.CreateAsync(new User { Id = 0, Name = "A", Username = "a", Email = "a@test.com" });
        await _repository.CreateAsync(new User { Id = 0, Name = "B", Username = "b", Email = "b@test.com" });

        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(2);
    }
}
