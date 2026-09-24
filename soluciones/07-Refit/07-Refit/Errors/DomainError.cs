namespace _07_Refit.Errors;

/// <summary>
/// Tipos de error de dominio para el manejo de errores con ROP (Result Object Pattern).
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
}
