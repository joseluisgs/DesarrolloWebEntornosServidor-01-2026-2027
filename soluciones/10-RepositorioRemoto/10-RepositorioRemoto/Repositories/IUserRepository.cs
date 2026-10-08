using _10_RepositorioRemoto.Models;

namespace _10_RepositorioRemoto.Repositories;

/// <summary>
/// Interfaz para el repositorio de usuarios en la BD local.
/// </summary>
public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task DeleteAllAsync();
    Task<int> CountAsync();
}
