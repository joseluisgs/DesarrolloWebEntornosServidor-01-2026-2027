namespace _05_MongoDB_Driver_EFCore.Config;

/// <summary>
/// Configuración de MongoDB.
/// Se usa con IOptions&lt;MongoConfig&gt; para leer appsettings.json.
/// </summary>
public class MongoConfig
{
    /// <summary>Cadena de conexión a MongoDB.</summary>
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    /// <summary>Nombre de la base de datos.</summary>
    public string DatabaseName { get; set; } = "productos_db";

    /// <summary>
    /// Si es true, vacía las colecciones al arrancar (solo para desarrollo/clase).
    /// ⚠️ NUNCA poner en true en producción.
    /// </summary>
    public bool DropData { get; set; } = false;
}
