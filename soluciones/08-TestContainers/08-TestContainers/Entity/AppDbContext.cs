using Microsoft.EntityFrameworkCore;
using _08_TestContainers.Entity;

namespace _08_TestContainers.Entity;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos de productos.
/// Configura el mapeo de la entidad ProductoEntity a la tabla 'productos'.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// DbSet de productos. Cada fila es un ProductoEntity.
    /// </summary>
    public DbSet<ProductoEntity> Productos => Set<ProductoEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductoEntity>(entity =>
        {
            entity.ToTable("productos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Precio).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });
    }
}
