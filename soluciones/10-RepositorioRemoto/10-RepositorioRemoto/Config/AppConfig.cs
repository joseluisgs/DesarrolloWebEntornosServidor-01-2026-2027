namespace _10_RepositorioRemoto.Config;

/// <summary>
/// Configuración de la aplicación (sección "ApiSettings" de appsettings.json).
/// Se usa con IOptions&lt;AppConfig&gt; en los servicios.
/// </summary>
public class AppConfig
{
    /// <summary>URL base de la API REST remota.</summary>
    public string ApiBaseUrl { get; set; } = "https://jsonplaceholder.typicode.com";

    /// <summary>Intervalo de sincronización en segundos.</summary>
    public int SyncIntervalSeconds { get; set; } = 60;

    /// <summary>Minutos de expiración de la caché.</summary>
    public int CacheExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Si es true, vacía las tablas al arrancar (solo para desarrollo/clase).
    /// ⚠️ NUNCA poner en true en producción.
    /// </summary>
    public bool DropData { get; set; } = false;
}
