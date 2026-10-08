namespace _10_RepositorioRemoto.Dto;

/// <summary>
/// DTO de solicitud para crear un usuario.
/// </summary>
public record CreateUserRequest
{
    public required string Name { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
