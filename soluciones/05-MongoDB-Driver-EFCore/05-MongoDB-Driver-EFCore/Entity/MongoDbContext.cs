using Microsoft.EntityFrameworkCore;
using _05_MongoDB_Driver_EFCore.Models;

namespace _05_MongoDB_Driver_EFCore.Entity;

/// <summary>
/// Contexto de EF Core para MongoDB.
/// EF Core soporta MongoDB como proveedor de datos.
/// </summary>
public class MongoDbContext(DbContextOptions<MongoDbContext> options) : DbContext(options)
{
    /// <summary>
    /// DbSet de productos.
    /// </summary>
    public DbSet<Producto> Productos => Set<Producto>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired();
        });
    }
}
