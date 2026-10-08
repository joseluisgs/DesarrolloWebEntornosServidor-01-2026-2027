using _10_RepositorioRemoto.Entity;
using _10_RepositorioRemoto.Models;
using Microsoft.EntityFrameworkCore;

namespace _10_RepositorioRemoto.Repositories;

/// <summary>
/// Repositorio de usuarios con Entity Framework Core + SQLite.
/// </summary>
public class UserRepository(AppDbContext context) : IUserRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        var entities = await context.Users.AsNoTracking().ToListAsync();
        return entities.Select(e => new User
        {
            Id = e.Id,
            Name = e.Name,
            Username = e.Username,
            Email = e.Email
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        var entity = await context.Users.FindAsync(id);
        return entity is null ? null : new User
        {
            Id = entity.Id,
            Name = entity.Name,
            Username = entity.Username,
            Email = entity.Email
        };
    }

    /// <inheritdoc />
    public async Task<User> CreateAsync(User user)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email
        };
        context.Users.Add(entity);
        await context.SaveChangesAsync();
        return user;
    }

    /// <inheritdoc />
    public async Task<User?> UpdateAsync(User user)
    {
        var entity = await context.Users.FindAsync(user.Id);
        if (entity is null) return null;

        entity.Name = user.Name;
        entity.Username = user.Username;
        entity.Email = user.Email;
        await context.SaveChangesAsync();

        return user;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await context.Users.FindAsync(id);
        if (entity is null) return false;

        context.Users.Remove(entity);
        await context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task DeleteAllAsync()
    {
        await context.Users.ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    public async Task<int> CountAsync()
    {
        return await context.Users.CountAsync();
    }
}
