namespace _10_RepositorioRemoto.Dto;

/// <summary>
/// DTO de respuesta para devolver usuarios al cliente.
/// </summary>
public record UserResponseDto
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
