// ============================================================
// SOLID - S: Single Responsibility Principle (SRP)
// Cada clase tiene UNA sola responsabilidad
// ============================================================

namespace _11_PatronesDiseno.SOLID;

// ❌ MALO: Una clase que hace TODO
public class PedidoMal
{
    public void Guardar(PedidoMal pedido) { /* guardar en BD */ }
    public void EnviarEmail(string email, string mensaje) { /* enviar email */ }
    public void GenerarFactura(PedidoMal pedido) { /* generar factura */ }
    public void NotificarAdmin(string mensaje) { /* notificar admin */ }
}

// ✅ BUENO: Cada clase con UNA responsabilidad
public record Pedido(string Producto, decimal Precio, string ClienteEmail);

public class PedidoRepository
{
    public void Guardar(Pedido pedido) { /* guardar en BD */ }
}

public class EmailService
{
    public void Enviar(string email, string mensaje) { /* enviar email */ }
}

public class FacturaService
{
    public string Generar(Pedido pedido) => $"Factura: {pedido.Producto} - {pedido.Precio}€";
}

public class NotificacionService
{
    public void NotificarAdmin(string mensaje) { /* notificar admin */ }
}
