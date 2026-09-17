using Microsoft.EntityFrameworkCore;

namespace _15_SincroniaVsAsyncronia;

public class ProductosDbContext(string dbPath) : DbContext
{
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired();
            entity.Property(e => e.Categoria).IsRequired();
            entity.Property(e => e.Proveedor).IsRequired();
        });
    }
}
