namespace _10_RepositorioRemoto.Config;

/// <summary>
/// Infraestructura que la aplicación usará según el entorno
/// (sección "InfraSettings" de appsettings.{Entorno}.json).
/// Es la pieza que decide qué implementación se registra en la Inyección de Dependencias.
/// </summary>
public class InfraSettings
{
    /// <summary>
    /// Proveedor de base de datos: <c>Sqlite</c> (Development) o <c>PostgreSql</c> (Production).
    /// </summary>
    public string Database { get; set; } = DatabaseProviders.Sqlite;

    /// <summary>
    /// Proveedor de caché: <c>Memory</c> (Development) o <c>Redis</c> (Production).
    /// </summary>
    public string Cache { get; set; } = CacheProviders.Memory;

    /// <summary>Cadenas de conexión disponibles (solo se usa la del proveedor seleccionado).</summary>
    public ConnectionStrings ConnectionStrings { get; set; } = new();
}

/// <summary>
/// Valores admitidos para <see cref="InfraSettings.Database"/>.
/// Se agrupan en una clase estática para evitar strings mágicos repartidos por el código.
/// </summary>
public static class DatabaseProviders
{
    /// <summary>SQLite: fichero local, sin infraestructura. Para desarrollo.</summary>
    public const string Sqlite = "Sqlite";

    /// <summary>PostgreSQL: servidor de bases de datos. Para producción.</summary>
    public const string PostgreSql = "PostgreSql";
}

/// <summary>
/// Valores admitidos para <see cref="InfraSettings.Cache"/>.
/// </summary>
public static class CacheProviders
{
    /// <summary>MemoryCache: caché en el proceso. Para desarrollo.</summary>
    public const string Memory = "Memory";

    /// <summary>Redis: caché externa compartida. Para producción.</summary>
    public const string Redis = "Redis";
}

/// <summary>
/// Cadenas de conexión de cada infraestructura.
/// En producción se sobreescriben con variables de entorno (doble guion bajo para anidar).
/// </summary>
public class ConnectionStrings
{
    /// <summary>Conexión a SQLite (Development).</summary>
    public string Sqlite { get; set; } = "Data Source=users.db";

    /// <summary>Conexión a PostgreSQL (Production).</summary>
    public string PostgreSql { get; set; } =
        "Host=localhost;Port=5432;Database=usuarios;Username=postgres;Password=postgres";

    /// <summary>Conexión a Redis (Production).</summary>
    public string Redis { get; set; } = "localhost:6379";
}
