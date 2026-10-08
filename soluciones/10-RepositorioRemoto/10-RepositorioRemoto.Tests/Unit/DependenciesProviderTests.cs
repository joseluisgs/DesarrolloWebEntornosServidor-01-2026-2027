using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using _10_RepositorioRemoto.Cache;
using _10_RepositorioRemoto.Config;
using _10_RepositorioRemoto.Infrastructure;
using _10_RepositorioRemoto.Services;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests de la Inyección de Dependencias condicional de <see cref="DependenciesProvider"/>.
/// Comprueba que la sección InfraSettings del appsettings activo decide
/// qué implementación concreta de BD y caché se registra en el contenedor.
/// </summary>
[TestFixture]
public class DependenciesProviderTests
{
    /// <summary>
    /// Construye una configuración en memoria como haría la app con el appsettings del entorno indicado.
    /// </summary>
    private static IConfiguration BuildConfiguration(string database, string cache)
    {
        var datos = new Dictionary<string, string?>
        {
            ["InfraSettings:Database"] = database,
            ["InfraSettings:Cache"] = cache,
            ["InfraSettings:ConnectionStrings:Sqlite"] = "Data Source=:memory:",
            ["InfraSettings:ConnectionStrings:PostgreSql"] = "Host=localhost;Port=5432;Database=usuarios",
            ["InfraSettings:ConnectionStrings:Redis"] = "localhost:6379",
            ["ApiSettings:ApiBaseUrl"] = "https://jsonplaceholder.typicode.com"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(datos)
            .Build();
    }

    [TestFixture]
    public class EntornoDevelopment : DependenciesProviderTests
    {
        [Test]
        public void ConfigureServices_Dev_RegistraMemoryCache()
        {
            // Arrange
            var configuration = BuildConfiguration(DatabaseProviders.Sqlite, CacheProviders.Memory);
            var services = new ServiceCollection();

            // Act
            DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            services.Should().Contain(s =>
                s.ServiceType == typeof(ICacheService) &&
                s.ImplementationType == typeof(MemoryCacheService));
        }

        [Test]
        public void ConfigureServices_Dev_NoRegistraRedis()
        {
            // Arrange
            var configuration = BuildConfiguration(DatabaseProviders.Sqlite, CacheProviders.Memory);
            var services = new ServiceCollection();

            // Act
            DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            services.Should().NotContain(s => s.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
        }
    }

    [TestFixture]
    public class EntornoProduction : DependenciesProviderTests
    {
        [Test]
        public void ConfigureServices_Prod_RegistraRedisCache()
        {
            // Arrange
            var configuration = BuildConfiguration(DatabaseProviders.PostgreSql, CacheProviders.Redis);
            var services = new ServiceCollection();

            // Act
            DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            services.Should().Contain(s =>
                s.ServiceType == typeof(ICacheService) &&
                s.ImplementationType == typeof(RedisCacheService));
        }

        [Test]
        public void ConfigureServices_Prod_RegistraMultiplexerDeRedis()
        {
            // Arrange
            var configuration = BuildConfiguration(DatabaseProviders.PostgreSql, CacheProviders.Redis);
            var services = new ServiceCollection();

            // Act
            DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            services.Should().Contain(s => s.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
        }
    }

    [TestFixture]
    public class Comun : DependenciesProviderTests
    {
        [Test]
        public void ConfigureServices_CualquierEntorno_RegistraLosMismosServiciosComunes()
        {
            // Arrange
            var configuration = BuildConfiguration(DatabaseProviders.Sqlite, CacheProviders.Memory);
            var services = new ServiceCollection();

            // Act
            DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            services.Should().Contain(s => s.ServiceType == typeof(IUserService));
            services.Should().Contain(s => s.ServiceType == typeof(Repositories.IUserRepository));
            services.Should().Contain(s => s.ServiceType == typeof(App));
        }

        [Test]
        public void ConfigureServices_RegistraInfraSettingsEnElContenedor()
        {
            // Arrange
            var configuration = BuildConfiguration(DatabaseProviders.PostgreSql, CacheProviders.Redis);
            var services = new ServiceCollection();

            // Act
            DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            services.Should().Contain(s => s.ServiceType == typeof(InfraSettings));
        }

        [Test]
        public void ConfigureServices_ProveedorDeCacheDesconocido_LanzaExcepcion()
        {
            // Arrange
            var configuration = BuildConfiguration(DatabaseProviders.Sqlite, "Memcached");
            var services = new ServiceCollection();

            // Act
            var accion = () => DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            accion.Should().Throw<InvalidOperationException>()
                .WithMessage("*Memcached*");
        }

        [Test]
        public void ConfigureServices_ProveedorDeBaseDeDatosDesconocido_LanzaExcepcion()
        {
            // Arrange
            var configuration = BuildConfiguration("MongoDb", CacheProviders.Memory);
            var services = new ServiceCollection();

            // Act
            var accion = () => DependenciesProvider.ConfigureServices(services, configuration);

            // Assert
            accion.Should().Throw<InvalidOperationException>()
                .WithMessage("*MongoDb*");
        }
    }
}
