namespace _10_RepositorioRemoto.Errors;

/// <summary>
/// Tipos de error de dominio para el manejo de errores con ROP (Result Object Pattern).
/// Patrón abstract record con nested records para tipos específicos de error.
/// </summary>
public abstract record DomainError
{
    /// <summary>
    /// Error cuando no se encuentra un recurso.
    /// </summary>
    public sealed record NotFound(string Resource, int Id) : DomainError;

    /// <summary>
    /// Error de validación de datos.
    /// </summary>
    public sealed record ValidationError(string Field, string Message) : DomainError;

    /// <summary>
    /// Error de comunicación con la API externa.
    /// </summary>
    public sealed record ApiError(int StatusCode, string Detail) : DomainError;

    /// <summary>
    /// Error genérico con mensaje personalizado.
    /// </summary>
    public sealed record Generic(string Message) : DomainError;
}
