using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using _10_RepositorioRemoto.Config;
using _10_RepositorioRemoto.Services;
using _10_RepositorioRemoto.Sync;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace _10_RepositorioRemoto.Tests;

/// <summary>
/// Tests de UserSyncBackgroundService.
/// Como es singleton y consume un servicio scoped, se inyecta IServiceScopeFactory;
/// aquí se mockea ese ámbito completo para poder testearlo en aislamiento.
/// </summary>
[TestFixture]
public class UserSyncBackgroundServiceTests
{
    private Mock<IServiceScopeFactory> _mockScopeFactory = null!;
    private Mock<IServiceScope> _mockScope = null!;
    private Mock<IServiceProvider> _mockProvider = null!;
    private Mock<IUserService> _mockUserService = null!;
    private Mock<ILogger<UserSyncBackgroundService>> _mockLogger = null!;
    private IOptions<AppConfig> _config = null!;

    [SetUp]
    public void SetUp()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserSyncBackgroundService>>();
        _config = Options.Create(new AppConfig { SyncIntervalSeconds = 1 });

        // Ámbito falso: CreateScope() devuelve un scope cuyo provider resuelve IUserService
        _mockProvider = new Mock<IServiceProvider>();
        _mockProvider.Setup(p => p.GetService(typeof(IUserService)))
            .Returns(_mockUserService.Object);

        _mockScope = new Mock<IServiceScope>();
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockProvider.Object);

        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
    }

    [Test]
    public async Task ExecuteAsync_IniciaSincronizacion_LlamaASyncFromRemote()
    {
        // Arrange
        _mockUserService
            .Setup(s => s.SyncFromRemoteAsync())
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        var service = new UserSyncBackgroundService(
            _mockScopeFactory.Object,
            _config,
            _mockLogger.Object);

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert
        _mockUserService.Verify(s => s.SyncFromRemoteAsync(), Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_CreaUnAmbitoPorCiclo()
    {
        // Arrange
        _mockUserService
            .Setup(s => s.SyncFromRemoteAsync())
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        var service = new UserSyncBackgroundService(
            _mockScopeFactory.Object,
            _config,
            _mockLogger.Object);

        // Act
        _ = service.StartAsync(cts.Token);
        await Task.Delay(120);
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert: el ámbito se pide en cada ciclo de sincronización
        _mockScopeFactory.Verify(f => f.CreateScope(), Times.AtLeastOnce);
    }

    [Test]
    public async Task ExecuteAsync_SiSyncFalla_NoPropagaExcepcion()
    {
        // Arrange
        _mockUserService
            .Setup(s => s.SyncFromRemoteAsync())
            .ThrowsAsync(new Exception("Error de red"));

        var cts = new CancellationTokenSource();

        var service = new UserSyncBackgroundService(
            _mockScopeFactory.Object,
            _config,
            _mockLogger.Object);

        // Act & Assert
        _ = service.StartAsync(cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        await service.Invoking(s => s.StopAsync(CancellationToken.None))
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task ExecuteAsync_CuandoSeCancela_NoSigueEjecutando()
    {
        // Arrange
        int callCount = 0;
        _mockUserService
            .Setup(s => s.SyncFromRemoteAsync())
            .Callback(() => callCount++)
            .Returns(Task.CompletedTask);

        var cts = new CancellationTokenSource();

        var service = new UserSyncBackgroundService(
            _mockScopeFactory.Object,
            _config,
            _mockLogger.Object);

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(150);
        var countAfterFirstRun = callCount;
        cts.Cancel();
        await Task.Delay(200);
        var countAfterCancel = callCount;

        // Assert
        countAfterFirstRun.Should().BeGreaterThanOrEqualTo(1);
        countAfterCancel.Should().Be(countAfterFirstRun);
    }
}
