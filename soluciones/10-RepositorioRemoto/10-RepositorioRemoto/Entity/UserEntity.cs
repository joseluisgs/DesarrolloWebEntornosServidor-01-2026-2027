namespace _10_RepositorioRemoto.Entity;

/// <summary>
/// Entidad de persistencia para Entity Framework Core.
/// </summary>
public class UserEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}
