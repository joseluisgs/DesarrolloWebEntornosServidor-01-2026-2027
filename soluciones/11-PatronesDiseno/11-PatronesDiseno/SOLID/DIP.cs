// ============================================================
// SOLID - D: Dependency Inversion Principle (DIP)
// Los módulos de alto nivel NO deben depender de los de bajo nivel.
// Ambos deben depender de abstracciones (interfaces).
// ============================================================

namespace _11_PatronesDiseno.SOLID;

// ❌ MALO: El servicio depende directamente de la implementación
public class PedidoServiceMal
{
    private readonly PedidoRepository _repository = new(); // ¡Acoplamiento duro!
    private readonly EmailService _email = new();          // ¡Otra dependencia directa!

    public void CrearPedido(Pedido pedido)
    {
        _repository.Guardar(pedido);
        _email.Enviar(pedido.ClienteEmail, $"Pedido {pedido.Producto} creado");
    }
}

// ✅ BUENO: Ambos dependen de abstracciones (interfaces)
public interface IPedidoRepository
{
    void Guardar(Pedido pedido);
}

public interface IEmailService
{
    void Enviar(string email, string mensaje);
}

public class PedidoRepositoryDip : IPedidoRepository
{
    public void Guardar(Pedido pedido) => Console.WriteLine($"Guardado: {pedido.Producto}");
}

public class EmailServiceDip : IEmailService
{
    public void Enviar(string email, string mensaje) => Console.WriteLine($"Email a {email}: {mensaje}");
}

// El servicio depende de INTERFACES, no de implementaciones
public class PedidoServiceDip(IPedidoRepository repository, IEmailService email)
{
    public void CrearPedido(Pedido pedido)
    {
        repository.Guardar(pedido);
        email.Enviar(pedido.ClienteEmail, $"Pedido {pedido.Producto} creado");
    }
}
