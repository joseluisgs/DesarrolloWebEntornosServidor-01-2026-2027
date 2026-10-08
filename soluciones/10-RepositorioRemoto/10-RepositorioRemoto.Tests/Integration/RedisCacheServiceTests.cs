using FluentAssertions;
using NUnit.Framework;
using StackExchange.Redis;
using Testcontainers.Redis;
using _10_RepositorioRemoto.Cache;
using _10_RepositorioRemoto.Config;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests de integración de <see cref="RedisCacheService"/> contra <b>Redis real</b>
/// usando Testcontainers. Comprueba que la implementación de PRODUCCIÓN se comporta
/// igual que la de desarrollo (MemoryCache), serializando a JSON.
/// </summary>
[TestFixture]
[Category("Integration")]
public class RedisCacheServiceTests
{
    private RedisContainer _container = null!;
    private IConnectionMultiplexer _redis = null!;
    private RedisCacheService _cacheService = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        await _container.StartAsync();

        // Misma cadena que usa InfraSettings en Production
        var infra = new InfraSettings
        {
            Cache = CacheProviders.Redis,
            ConnectionStrings = new ConnectionStrings { Redis = _container.GetConnectionString() }
        };

        _redis = await ConnectionMultiplexer.ConnectAsync(infra.ConnectionStrings.Redis);
        _cacheService = new RedisCacheService(_redis);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_redis is not null)
        {
            await _redis.CloseAsync();
            _redis.Dispose();
        }

        await _container.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Caché limpia entre tests
        await _cacheService.ClearAsync();
    }

    private record PersonaDto(string Nombre, int Edad);

    [Test]
    public async Task SetAsync_GetAsync_ClaveExistente_RetornaValorSerializado()
    {
        // Arrange
        var persona = new PersonaDto("Ana García", 28);

        // Act
        await _cacheService.SetAsync("persona:1", persona);
        var resultado = await _cacheService.GetAsync<PersonaDto>("persona:1");

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Ana García");
        resultado.Edad.Should().Be(28);
    }

    [Test]
    public async Task GetAsync_ClaveInexistente_RetornaNull()
    {
        // Act
        var resultado = await _cacheService.GetAsync<PersonaDto>("persona:inexistente");

        // Assert
        resultado.Should().BeNull();
    }

    [Test]
    public async Task SetAsync_ConExpiracion_CaducaSegunLoIndicado()
    {
        // Arrange
        await _cacheService.SetAsync("temporal", "valor", TimeSpan.FromMilliseconds(300));

        // Act: primero está, después expira
        var antes = await _cacheService.GetAsync<string>("temporal");
        await Task.Delay(600);
        var despues = await _cacheService.GetAsync<string>("temporal");

        // Assert
        antes.Should().Be("valor");
        despues.Should().BeNull();
    }

    [Test]
    public async Task RemoveAsync_ClaveExistente_LaBorra()
    {
        // Arrange
        await _cacheService.SetAsync("borrable", "valor");

        // Act
        await _cacheService.RemoveAsync("borrable");

        // Assert
        (await _cacheService.GetAsync<string>("borrable")).Should().BeNull();
    }

    [Test]
    public async Task ClearAsync_ConVariasClaves_LasBorraTodas()
    {
        // Arrange
        await _cacheService.SetAsync("users:1", "Ana");
        await _cacheService.SetAsync("users:2", "Bob");
        await _cacheService.SetAsync("users:all", new[] { "Ana", "Bob" });

        // Act
        await _cacheService.ClearAsync();

        // Assert
        (await _cacheService.GetAsync<string>("users:1")).Should().BeNull();
        (await _cacheService.GetAsync<string>("users:2")).Should().BeNull();
        (await _cacheService.GetAsync<string[]>("users:all")).Should().BeNull();
    }

    [Test]
    public async Task SetAsync_ValorDeLista_SerializaYRecuperaLaColeccion()
    {
        // Arrange
        var usuarios = new List<string> { "Ana", "Bob", "Carlos" };

        // Act
        await _cacheService.SetAsync("users:all", usuarios);
        var resultado = await _cacheService.GetAsync<List<string>>("users:all");

        // Assert
        resultado.Should().BeEquivalentTo(usuarios);
    }
}
