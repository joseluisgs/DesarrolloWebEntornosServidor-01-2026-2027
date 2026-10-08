using _10_RepositorioRemoto.Models;
using _10_RepositorioRemoto.Dto;

namespace _10_RepositorioRemoto.Mappers;

/// <summary>
/// Funciones de extensión para mapear entre User y DTOs.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Convierte un User a UserResponseDto.
    /// </summary>
    public static UserResponseDto ToResponse(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Username = user.Username,
        Email = user.Email
    };

    /// <summary>
    /// Convierte un CreateUserRequest a User.
    /// </summary>
    public static User ToModel(this CreateUserRequest request) => new()
    {
        Id = 0,
        Name = request.Name,
        Username = request.Username,
        Email = request.Email
    };

    /// <summary>
    /// Convierte un JsonPlaceholderUserDto a User.
    /// </summary>
    public static User ToModel(this JsonPlaceholderUserDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Username = dto.Username,
        Email = dto.Email
    };

    /// <summary>
    /// Convierte una lista de Users a lista de UserResponseDto.
    /// </summary>
    public static List<UserResponseDto> ToResponse(this IEnumerable<User> users)
        => users.Select(u => u.ToResponse()).ToList();
}
