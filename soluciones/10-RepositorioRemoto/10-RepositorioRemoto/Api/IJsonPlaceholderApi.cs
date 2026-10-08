using _10_RepositorioRemoto.Dto;
using Refit;

namespace _10_RepositorioRemoto.Api;

/// <summary>
/// Interfaz tipada para consumir JSONPlaceholder API con Refit.
/// </summary>
public interface IJsonPlaceholderApi
{
    [Get("/users")]
    Task<IReadOnlyList<JsonPlaceholderUserDto>> GetUsersAsync();

    [Get("/users/{id}")]
    Task<JsonPlaceholderUserDto> GetUserByIdAsync(int id);

    [Post("/users")]
    Task<JsonPlaceholderUserDto> CreateUserAsync([Body] JsonPlaceholderUserDto user);

    [Put("/users/{id}")]
    Task UpdateUserAsync(int id, [Body] JsonPlaceholderUserDto user);

    [Delete("/users/{id}")]
    Task DeleteUserAsync(int id);
}
