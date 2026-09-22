using Microsoft.EntityFrameworkCore;

namespace _04_PostgreSQL_Dapper_EFCore.Entity;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos de productos.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// DbSet de productos.
    /// </summary>
    public DbSet<ProductoEntity> Productos => Set<ProductoEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductoEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired();
        });
    }
}
