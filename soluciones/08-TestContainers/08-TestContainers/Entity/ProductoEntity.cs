using System.ComponentModel.DataAnnotations.Schema;

namespace _08_TestContainers.Entity;

/// <summary>
/// Entidad de persistencia para Entity Framework Core.
/// Mapea la tabla 'productos' en PostgreSQL.
/// </summary>
[Table("productos")]
public class ProductoEntity
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [Column("nombre")]
    public required string Nombre { get; set; }

    /// <summary>
    /// Precio del producto.
    /// </summary>
    [Column("precio")]
    public decimal Precio { get; set; }

    /// <summary>
    /// Categoría del producto.
    /// </summary>
    [Column("categoria")]
    public required string Categoria { get; set; }

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
