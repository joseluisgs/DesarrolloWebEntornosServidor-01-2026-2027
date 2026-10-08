namespace _10_RepositorioRemoto.Models;

/// <summary>
/// Modelo de dominio que representa un usuario en el sistema.
/// </summary>
public record User
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}
