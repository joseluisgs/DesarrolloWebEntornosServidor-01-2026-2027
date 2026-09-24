namespace _07_Refit.Models;

/// <summary>
/// Modelo que representa un usuario de JSONPlaceholder.
/// Mapea directamente la respuesta de la API REST.
/// </summary>
public record Usuario(
    int Id,
    string Name,
    string Username,
    string Email
);
