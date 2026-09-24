namespace _07_Refit.Config;

/// <summary>
/// Configuración de la API externa.
/// Se usa con IOptions&lt;ApiConfig&gt; para leer appsettings.json.
/// </summary>
public class ApiConfig
{
    /// <summary>URL base de la API REST.</summary>
    public string BaseUrl { get; set; } = "https://jsonplaceholder.typicode.com";
}
