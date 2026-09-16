namespace ProductoApp.Models;

/// <summary>
/// Modelo que representa un producto en el sistema.
/// </summary>
/// <param name="Id">Identificador único del producto.</param>
/// <param name="Nombre">Nombre del producto.</param>
/// <param name="Precio">Precio del producto.</param>
public record Producto(int Id, string Nombre, decimal Precio);
