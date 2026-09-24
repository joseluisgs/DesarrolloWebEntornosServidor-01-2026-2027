using Refit;
using _07_Refit.Dto;
using _07_Refit.Models;

namespace _07_Refit.Api;

/// <summary>
/// Interfaz que define los endpoints de JSONPlaceholder.
/// Refit genera la implementación en tiempo de compilación
/// a partir de los atributos [Get], [Post], [Put], [Delete].
/// </summary>
[Headers("Content-Type: application/json")]
public interface IJsonPlaceholderApi
{
    /// <summary>
    /// GET /users - Obtiene todos los usuarios.
    /// </summary>
    [Get("/users")]
    Task<List<Usuario>> GetUsuariosAsync();

    /// <summary>
    /// GET /users/{id} - Obtiene un usuario por su ID.
    /// </summary>
    [Get("/users/{id}")]
    Task<Usuario?> GetUsuarioByIdAsync(int id);

    /// <summary>
    /// POST /users - Crea un nuevo usuario.
    /// El servidor asigna el Id automáticamente.
    /// </summary>
    [Post("/users")]
    Task<Usuario> CreateUsuarioAsync([Body] CreateUserRequest request);

    /// <summary>
    /// PUT /users/{id} - Actualiza completamente un usuario.
    /// </summary>
    [Put("/users/{id}")]
    Task<Usuario> UpdateUsuarioAsync(int id, [Body] UpdateUserRequest request);

    /// <summary>
    /// DELETE /users/{id} - Elimina un usuario.
    /// </summary>
    [Delete("/users/{id}")]
    Task DeleteUsuarioAsync(int id);
}
