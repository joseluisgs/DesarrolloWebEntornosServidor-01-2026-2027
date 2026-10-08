using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using _10_RepositorioRemoto.Entity;
using _10_RepositorioRemoto.Models;
using _10_RepositorioRemoto.Repositories;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests de UserRepository con SQLite en memoria.
/// </summary>
[TestFixture]
public class UserRepositoryTests
{
    private AppDbContext _context = null!;
    private UserRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _context = new AppDbContext(options);
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();

        _repository = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
    }

    [TestFixture]
    public class CreateAsync : UserRepositoryTests
    {
        [Test]
        public async Task CreateAsync_UsuarioValido_RetornaUsuarioConId()
        {
            // Arrange
            var user = new User
            {
                Id = 0,
                Name = "Ana García",
                Username = "anagarcia",
                Email = "ana@email.com"
            };

            // Act
            var result = await _repository.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Ana García");
        }

        [Test]
        public async Task CreateAsync_UsuarioValido_SeGuardaEnBD()
        {
            // Arrange
            var user = new User
            {
                Id = 0,
                Name = "Carlos López",
                Username = "carloslopez",
                Email = "carlos@email.com"
            };

            // Act
            await _repository.CreateAsync(user);
            var all = await _repository.GetAllAsync();

            // Assert
            all.Should().HaveCount(1);
            all[0].Name.Should().Be("Carlos López");
        }
    }

    [TestFixture]
    public class GetAllAsync : UserRepositoryTests
    {
        [Test]
        public async Task GetAllAsync_SinDatos_RetornaListaVacia()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
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
    }

    [TestFixture]
    public class GetByIdAsync : UserRepositoryTests
    {
        [Test]
        public async Task GetByIdAsync_Existente_RetornaUsuario()
        {
            // Arrange
            await _repository.CreateAsync(new User
            {
                Id = 0,
                Name = "Ana",
                Username = "ana",
                Email = "ana@test.com"
            });
            var all = await _repository.GetAllAsync();
            var created = all.First();

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
    }

    [TestFixture]
    public class UpdateAsync : UserRepositoryTests
    {
        [Test]
        public async Task UpdateAsync_Existente_RetornaUsuarioActualizado()
        {
            // Arrange
            await _repository.CreateAsync(new User
            {
                Id = 0,
                Name = "Ana",
                Username = "ana",
                Email = "ana@test.com"
            });
            var all = await _repository.GetAllAsync();
            var created = all.First();

            var updated = new User
            {
                Id = created.Id,
                Name = "Ana García",
                Username = "anagarcia",
                Email = "ana@email.com"
            };

            // Act
            var result = await _repository.UpdateAsync(updated);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Ana García");
        }

        [Test]
        public async Task UpdateAsync_Inexistente_RetornaNull()
        {
            // Arrange
            var user = new User { Id = 999, Name = "Ghost", Username = "ghost", Email = "ghost@test.com" };

            // Act
            var result = await _repository.UpdateAsync(user);

            // Assert
            result.Should().BeNull();
        }
    }

    [TestFixture]
    public class DeleteAsync : UserRepositoryTests
    {
        [Test]
        public async Task DeleteAsync_Existente_RetornaTrue()
        {
            // Arrange
            await _repository.CreateAsync(new User
            {
                Id = 0,
                Name = "Ana",
                Username = "ana",
                Email = "ana@test.com"
            });
            var all = await _repository.GetAllAsync();
            var created = all.First();

            // Act
            var result = await _repository.DeleteAsync(created.Id);

            // Assert
            result.Should().BeTrue();
            var remaining = await _repository.GetAllAsync();
            remaining.Should().BeEmpty();
        }

        [Test]
        public async Task DeleteAsync_Inexistente_RetornaFalse()
        {
            // Act
            var result = await _repository.DeleteAsync(999);

            // Assert
            result.Should().BeFalse();
        }
    }

    [TestFixture]
    public class CountAsync : UserRepositoryTests
    {
        [Test]
        public async Task CountAsync_SinDatos_RetornaCero()
        {
            // Act
            var result = await _repository.CountAsync();

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public async Task CountAsync_ConDatos_RetornaCantidad()
        {
            // Arrange
            await _repository.CreateAsync(new User { Id = 0, Name = "A", Username = "a", Email = "a@test.com" });
            await _repository.CreateAsync(new User { Id = 0, Name = "B", Username = "b", Email = "b@test.com" });
            await _repository.CreateAsync(new User { Id = 0, Name = "C", Username = "c", Email = "c@test.com" });

            // Act
            var result = await _repository.CountAsync();

            // Assert
            result.Should().Be(3);
        }
    }

    [TestFixture]
    public class DeleteAllAsync : UserRepositoryTests
    {
        [Test]
        public async Task DeleteAllAsync_ConDatos_EliminaTodos()
        {
            // Arrange
            await _repository.CreateAsync(new User { Id = 0, Name = "A", Username = "a", Email = "a@test.com" });
            await _repository.CreateAsync(new User { Id = 0, Name = "B", Username = "b", Email = "b@test.com" });

            // Act
            await _repository.DeleteAllAsync();
            var count = await _repository.CountAsync();

            // Assert
            count.Should().Be(0);
        }
    }
}
