namespace _08_TestContainers.Config;

/// <summary>
/// Configuración de la base de datos.
/// Se usa con IOptions&lt;DatabaseConfig&gt; para leer appsettings.json.
/// </summary>
public class DatabaseConfig
{
    /// <summary>Cadena de conexión a PostgreSQL.</summary>
    public string ConnectionString { get; set; } = "Host=localhost;Port=5432;Database=productos_db;Username=postgres;Password=postgres";

    /// <summary>
    /// Si es true, vacía las tablas al arrancar (solo para desarrollo/clase).
    /// ⚠️ NUNCA poner en true en producción.
    /// </summary>
    public bool DropData { get; set; } = false;
}
