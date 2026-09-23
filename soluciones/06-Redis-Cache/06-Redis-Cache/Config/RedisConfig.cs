namespace _06_Redis_Cache.Config;

/// <summary>
/// Configuración de Redis.
/// Se usa con IOptions&lt;RedisConfig&gt; para leer appsettings.json.
/// </summary>
public class RedisConfig
{
    /// <summary>Cadena de conexión a Redis (host:puerto).</summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Si es true, vacía la caché al arrancar (solo para desarrollo/clase).
    /// ⚠️ NUNCA poner en true en producción.
    /// </summary>
    public bool DropData { get; set; } = false;
}
