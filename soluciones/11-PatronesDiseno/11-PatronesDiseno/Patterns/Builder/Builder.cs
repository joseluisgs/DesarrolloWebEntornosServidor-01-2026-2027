// ============================================================
// PATRÓN BUILDER
// Construye objetos complejos paso a paso.
// Conecta con SOLID: SRP (separa construcción de representación)
// Ejemplo real: Host.CreateApplicationBuilder, HttpClientBuilder
// ============================================================

namespace _11_PatronesDiseno.Patterns.Builder;

/// <summary>
/// Objeto complejo que se construye con Builder.
/// </summary>
public class ServidorConfig
{
    public string Host { get; init; } = "localhost";
    public int Puerto { get; init; } = 80;
    public bool UsarHttps { get; init; } = false;
    public int TimeoutSegundos { get; init; } = 30;
    public int MaxConexiones { get; init; } = 100;
    public string? DirectorioLog { get; init; }
    public bool HabilitarCors { get; init; } = false;
    public string[] OrigenesPermitidos { get; init; } = [];

    public override string ToString() =>
        $"Host={Host}:{Puerto} HTTPS={UsarHttps} Timeout={TimeoutSegundos}s " +
        $"MaxConexiones={MaxConexiones} CORS={HabilitarCors} " +
        $"Origenes=[{string.Join(", ", OrigenesPermitidos)}]";
}

/// <summary>
/// Builder — construye ServidorConfig paso a paso.
/// Cada método retorna el builder para allow encadenamiento (fluent API).
/// </summary>
public class ServidorConfigBuilder
{
    private string _host = "localhost";
    private int _puerto = 80;
    private bool _usarHttps = false;
    private int _timeout = 30;
    private int _maxConexiones = 100;
    private string? _directorioLog;
    private bool _habilitarCors = false;
    private readonly List<string> _origenesPermitidos = [];

    /// <summary>
    /// Establece el host del servidor.
    /// </summary>
    public ServidorConfigBuilder ConHost(string host)
    {
        _host = host;
        return this;
    }

    /// <summary>
    /// Establece el puerto del servidor.
    /// </summary>
    public ServidorConfigBuilder ConPuerto(int puerto)
    {
        _puerto = puerto;
        return this;
    }

    /// <summary>
    /// Habilita HTTPS.
    /// </summary>
    public ServidorConfigBuilder ConHttps()
    {
        _usarHttps = true;
        return this;
    }

    /// <summary>
    /// Establece el timeout en segundos.
    /// </summary>
    public ServidorConfigBuilder ConTimeout(int segundos)
    {
        _timeout = segundos;
        return this;
    }

    /// <summary>
    /// Establece el máximo de conexiones simultáneas.
    /// </summary>
    public ServidorConfigBuilder ConMaxConexiones(int max)
    {
        _maxConexiones = max;
        return this;
    }

    /// <summary>
    /// Establece el directorio de logs.
    /// </summary>
    public ServidorConfigBuilder ConDirectorioLog(string directorio)
    {
        _directorioLog = directorio;
        return this;
    }

    /// <summary>
    /// Habilita CORS con los orígenes especificados.
    /// </summary>
    public ServidorConfigBuilder ConCors(params string[] origenes)
    {
        _habilitarCors = true;
        _origenesPermitidos.AddRange(origenes);
        return this;
    }

    /// <summary>
    /// Construye la configuración final.
    /// </summary>
    public ServidorConfig Build() => new()
    {
        Host = _host,
        Puerto = _puerto,
        UsarHttps = _usarHttps,
        TimeoutSegundos = _timeout,
        MaxConexiones = _maxConexiones,
        DirectorioLog = _directorioLog,
        HabilitarCors = _habilitarCors,
        OrigenesPermitidos = [.. _origenesPermitidos]
    };
}
