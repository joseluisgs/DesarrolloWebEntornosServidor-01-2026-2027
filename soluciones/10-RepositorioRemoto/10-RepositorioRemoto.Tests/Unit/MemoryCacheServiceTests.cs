using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using _10_RepositorioRemoto.Cache;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests unitarios de MemoryCacheService.
/// </summary>
[TestFixture]
public class MemoryCacheServiceTests
{
    private record PersonaDto(string Nombre, int Edad);

    private IMemoryCache _memoryCache = null!;
    private MemoryCacheService _cacheService = null!;

    [SetUp]
    public void SetUp()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new MemoryCacheService(_memoryCache);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryCache.Dispose();
    }

    [TestFixture]
    public class GetAsync : MemoryCacheServiceTests
    {
        [Test]
        public async Task GetAsync_ClaveExistente_RetornaValor()
        {
            // Arrange
            await _cacheService.SetAsync("user:1", "Ana");

            // Act
            var result = await _cacheService.GetAsync<string>("user:1");

            // Assert
            result.Should().Be("Ana");
        }

        [Test]
        public async Task GetAsync_ClaveInexistente_RetornaNull()
        {
            // Act
            var result = await _cacheService.GetAsync<string>("no-existe");

            // Assert
            result.Should().BeNull();
        }
    }

    [TestFixture]
    public class SetAsync : MemoryCacheServiceTests
    {
        [Test]
        public async Task SetAsync_DatoSimple_SeGuardaCorrectamente()
        {
            // Act
            await _cacheService.SetAsync("key", "value");
            var result = await _cacheService.GetAsync<string>("key");

            // Assert
            result.Should().Be("value");
        }

        [Test]
        public async Task SetAsync_Objeto_SeGuardaCorrectamente()
        {
            // Arrange
            var objeto = new PersonaDto("Ana", 25);

            // Act
            await _cacheService.SetAsync("objeto", objeto);
            var result = await _cacheService.GetAsync<PersonaDto>("objeto");

            // Assert
            result.Should().NotBeNull();
            result!.Nombre.Should().Be("Ana");
        }

        [Test]
        public async Task SetAsync_ConExpiracion_ExpiraCorrectamente()
        {
            // Arrange
            await _cacheService.SetAsync("temp", "dato", TimeSpan.FromMilliseconds(50));

            // Act - antes de expirar
            var antes = await _cacheService.GetAsync<string>("temp");

            // Wait for expiration
            await Task.Delay(100);

            // Act - después de expirar
            var despues = await _cacheService.GetAsync<string>("temp");

            // Assert
            antes.Should().Be("dato");
            despues.Should().BeNull();
        }
    }

    [TestFixture]
    public class RemoveAsync : MemoryCacheServiceTests
    {
        [Test]
        public async Task RemoveAsync_ClaveExistente_EliminaDato()
        {
            // Arrange
            await _cacheService.SetAsync("key", "value");

            // Act
            await _cacheService.RemoveAsync("key");
            var result = await _cacheService.GetAsync<string>("key");

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task RemoveAsync_ClaveInexistente_NoLanzaError()
        {
            // Act & Assert
            await _cacheService.Invoking(s => s.RemoveAsync("no-existe"))
                .Should().NotThrowAsync();
        }
    }

    [TestFixture]
    public class ClearAsync : MemoryCacheServiceTests
    {
        [Test]
        public async Task ClearAsync_ConDatos_LimpiaTodo()
        {
            // Arrange
            await _cacheService.SetAsync("key1", "value1");
            await _cacheService.SetAsync("key2", "value2");

            // Act
            await _cacheService.ClearAsync();

            // Assert
            var result1 = await _cacheService.GetAsync<string>("key1");
            var result2 = await _cacheService.GetAsync<string>("key2");
            result1.Should().BeNull();
            result2.Should().BeNull();
        }
    }
}
