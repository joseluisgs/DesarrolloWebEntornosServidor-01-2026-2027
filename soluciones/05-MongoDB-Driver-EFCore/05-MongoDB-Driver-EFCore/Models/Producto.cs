using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace _05_MongoDB_Driver_EFCore.Models;

/// <summary>
/// Modelo para MongoDB. Usa string como Id (ObjectId de MongoDB).
/// </summary>
public class Producto
{
    /// <summary>
    /// Identificador único de MongoDB (ObjectId).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Precio del producto.
    /// </summary>
    [BsonElement("precio")]
    public decimal Precio { get; set; }

    /// <summary>
    /// Categoría del producto.
    /// </summary>
    [BsonElement("categoria")]
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del documento.
    /// </summary>
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización.
    /// </summary>
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
