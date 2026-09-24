using _07_Refit.Dto;
using _07_Refit.Models;

namespace _07_Refit.Mappers;

/// <summary>
/// Funciones de extensión para mapear entre DTOs y modelos de dominio.
/// Centraliza la lógica de conversión entre capas.
/// </summary>
public static class UsuarioMapper
{
    /// <summary>
    /// Convierte un Usuario a CreateUserRequest.
    /// </summary>
    public static CreateUserRequest ToCreateRequest(this Usuario usuario) => new(
        usuario.Name,
        usuario.Username,
        usuario.Email);

    /// <summary>
    /// Convierte un Usuario a UpdateUserRequest.
    /// </summary>
    public static UpdateUserRequest ToUpdateRequest(this Usuario usuario) => new(
        usuario.Id,
        usuario.Name,
        usuario.Username,
        usuario.Email);

    /// <summary>
    /// Convierte un CreateUserRequest a Usuario.
    /// </summary>
    public static Usuario ToUsuario(this CreateUserRequest request, int id = 0) => new(
        id,
        request.Name,
        request.Username,
        request.Email);

    /// <summary>
    /// Convierte un UpdateUserRequest a Usuario.
    /// </summary>
    public static Usuario ToUsuario(this UpdateUserRequest request) => new(
        request.Id,
        request.Name,
        request.Username,
        request.Email);
}
