using _10_RepositorioRemoto.Config;
using _10_RepositorioRemoto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace _10_RepositorioRemoto.Sync;

/// <summary>
/// Servicio en background que sincroniza la BD local con la API remota cada N segundos.
/// Intervalo configurable en appsettings.json → ApiSettings:SyncIntervalSeconds.
/// </summary>
/// <remarks>
/// ⚠️ Es un servicio <b>singleton</b>, pero <see cref="IUserService"/> es <b>scoped</b>
/// (arrastra un AppDbContext con ámbito). Por eso NO se inyecta directamente:
/// se crea un ámbito nuevo en cada ciclo con <see cref="IServiceScopeFactory"/>.
/// </remarks>
public class UserSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<AppConfig> config,
    ILogger<UserSyncBackgroundService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(config.Value.SyncIntervalSeconds);
        logger.LogInformation("Servicio de sincronización iniciado (intervalo: {Interval}s)", interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("Ejecutando sincronización programada...");

                // Ámbito propio por cada ciclo: así el scoped IUserService
                // (y su AppDbContext) se crea y se libera correctamente.
                using var scope = scopeFactory.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                await userService.SyncFromRemoteAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error durante la sincronización programada");
            }

            await Task.Delay(interval, stoppingToken);
        }

        logger.LogInformation("Servicio de sincronización detenido");
    }
}
