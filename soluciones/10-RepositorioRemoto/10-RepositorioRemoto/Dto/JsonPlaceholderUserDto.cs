using System.Text.Json.Serialization;

namespace _10_RepositorioRemoto.Dto;

/// <summary>
/// DTO que representa la respuesta de JSONPlaceholder API.
/// </summary>
public record JsonPlaceholderUserDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("username")]
    public required string Username { get; init; }

    [JsonPropertyName("email")]
    public required string Email { get; init; }
}
