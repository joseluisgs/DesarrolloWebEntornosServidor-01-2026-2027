using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _04_PostgreSQL_Dapper_EFCore.Entity;

/// <summary>
/// Entidad de persistencia para Productos.
/// Extiende el modelo con campos de auditoría.
/// </summary>
[Table("productos")]
public class ProductoEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nombre")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Column("precio")]
    public decimal Precio { get; set; }

    [Required]
    [Column("categoria")]
    [MaxLength(100)]
    public string Categoria { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
