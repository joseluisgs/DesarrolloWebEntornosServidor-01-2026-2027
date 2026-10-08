using Microsoft.EntityFrameworkCore;

namespace _10_RepositorioRemoto.Entity;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos local de usuarios.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// DbSet de usuarios.
    /// </summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
        });
    }
}
