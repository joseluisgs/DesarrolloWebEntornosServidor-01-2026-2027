namespace _07_Refit.Dto;

/// <summary>
/// DTO para actualizar un usuario existente en la API.
/// </summary>
public record UpdateUserRequest(
    int Id,
    string Name,
    string Username,
    string Email
);
