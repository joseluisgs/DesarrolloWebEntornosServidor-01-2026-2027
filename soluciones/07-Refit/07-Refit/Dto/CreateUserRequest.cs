namespace _07_Refit.Dto;

/// <summary>
/// DTO para crear un nuevo usuario en la API.
/// El campo Id lo asigna el servidor.
/// </summary>
public record CreateUserRequest(
    string Name,
    string Username,
    string Email
);
