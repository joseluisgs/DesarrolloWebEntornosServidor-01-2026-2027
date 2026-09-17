// ============================================================
// SOLID - O: Open/Closed Principle (OCP)
// Abierto a extensión, cerrado a modificación
// ============================================================

namespace _11_PatronesDiseno.SOLID;

// ❌ MALO: Para añadir un tipo de notificación, hay que modificar la clase
public class NotificadorMal
{
    public void Notificar(string tipo, string mensaje)
    {
        if (tipo == "Email")
            Console.WriteLine($"Email: {mensaje}");
        else if (tipo == "SMS")
            Console.WriteLine($"SMS: {mensaje}");
        else if (tipo == "Push")
            Console.WriteLine($"Push: {mensaje}");
        // ¡Cada vez que añades un tipo, modificas esta clase!
    }
}

// ✅ BUENO: Abierto a extensión (nuevos tipos), cerrado a modificación
public interface INotificador
{
    string Tipo { get; }
    void Enviar(string mensaje);
}

public class EmailNotificador : INotificador
{
    public string Tipo => "Email";
    public void Enviar(string mensaje) => Console.WriteLine($"📧 Email: {mensaje}");
}

public class SmsNotificador : INotificador
{
    public string Tipo => "SMS";
    public void Enviar(string mensaje) => Console.WriteLine($"📱 SMS: {mensaje}");
}

public class PushNotificador : INotificador
{
    public string Tipo => "Push";
    public void Enviar(string mensaje) => Console.WriteLine($"🔔 Push: {mensaje}");
}

// Para añadir un nuevo tipo (ej: WhatsApp), solo creas una nueva clase
// NO modifies las existentes
