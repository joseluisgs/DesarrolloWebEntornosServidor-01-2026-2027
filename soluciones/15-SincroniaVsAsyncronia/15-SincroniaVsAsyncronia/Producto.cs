namespace _15_SincroniaVsAsyncronia;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public double Precio { get; set; }
    public int Stock { get; set; }
    public string Proveedor { get; set; } = "";

    public Producto() { }

    public Producto(int id, string nombre, string categoria, double precio, int stock, string proveedor)
    {
        Id = id;
        Nombre = nombre;
        Categoria = categoria;
        Precio = precio;
        Stock = stock;
        Proveedor = proveedor;
    }
}
