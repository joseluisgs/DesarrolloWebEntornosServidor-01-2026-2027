using _11_PatronesDiseno.Models;
using _11_PatronesDiseno.SOLID;
using _11_PatronesDiseno.Patterns.Strategy;
using _11_PatronesDiseno.Patterns.Factory;
using _11_PatronesDiseno.Patterns.Decorator;
using _11_PatronesDiseno.Patterns.Adapter;
using _11_PatronesDiseno.Patterns.Builder;

// ============================================================
// Ejemplo 11: Patrones de Diseño y SOLID
// ============================================================
Console.WriteLine("=== Ejemplo 11: Patrones de Diseño y SOLID ===\n");

// ============================================================
// PARTE 1: SOLID
// ============================================================

// --- SRP: Cada clase con una responsabilidad ---
Console.WriteLine("--- SRP: Single Responsibility Principle ---");
var repo = new PedidoRepository();
var email = new EmailService();
var factura = new FacturaService();
var notif = new NotificacionService();

var pedido = new Pedido("Café", 2.50m, "ana@email.com");
repo.Guardar(pedido);
email.Enviar(pedido.ClienteEmail, $"Tu pedido: {pedido.Producto}");
Console.WriteLine($"Factura: {factura.Generar(pedido)}");
notif.NotificarAdmin($"Nuevo pedido de {pedido.ClienteEmail}");
Console.WriteLine();

// --- OCP: Abierto a extensión, cerrado a modificación ---
Console.WriteLine("--- OCP: Open/Closed Principle ---");
var notificadores = new List<INotificador>
{
    new EmailNotificador(),
    new SmsNotificador(),
    new PushNotificador()
};

foreach (var n in notificadores)
    n.Enviar("Nuevo pedido recibido");
Console.WriteLine();

// --- ISP: Interfaces pequeñas y específicas ---
Console.WriteLine("--- ISP: Interface Segregation Principle ---");
IReadRepository<Persona> soloLectura = new PersonaReadOnlyRepository();

// Si intentas usar Create en soloLectura, no compila
// soloLectura.Create(...)  // ❌ Error de compilación — ¡eso es ISP!
Console.WriteLine("IReadRepository: solo lectura (Find, GetById, GetAll)");
Console.WriteLine("IWriteRepository: solo escritura (Create, Update, Delete)");
Console.WriteLine("Los clientes dependen solo de lo que necesitan");
Console.WriteLine();

// --- DIP: Depender de abstracciones ---
Console.WriteLine("--- DIP: Dependency Inversion Principle ---");
IPedidoRepository repoDip = new PedidoRepositoryDip();
IEmailService emailDip = new EmailServiceDip();
var serviceDip = new PedidoServiceDip(repoDip, emailDip);
serviceDip.CrearPedido(new Pedido("Te", 1.80m, "bob@email.com"));
Console.WriteLine();

// ============================================================
// PARTE 2: PATRONES
// ============================================================

// --- STRATEGY: Intercambiar algoritmos en runtime ---
Console.WriteLine("--- STRATEGY: Validación y Ordenación ---");

// Strategy de validación
IValidador<Persona> validador = new PersonaValidador();
var serviceStrategy = new PersonaServiceStrategy(validador);

var (personaValida, erroresOk) = serviceStrategy.Crear(1, "Ana", "ana@email.com", 25);
Console.WriteLine(personaValida is not null
    ? $"✅ Persona válida: {personaValida.Nombre}"
    : $"❌ Errores: {string.Join(", ", erroresOk)}");

var (personaInvalida, erroresFail) = serviceStrategy.Crear(2, "", "invalido", -5);
Console.WriteLine(personaInvalida is not null
    ? $"✅ Persona válida: {personaInvalida.Nombre}"
    : $"❌ Errores: {string.Join(", ", erroresFail)}");

// Strategy de ordenación
var personas = new List<Persona>
{
    new(1, "Carlos", "c@email.com", 30),
    new(2, "Ana", "a@email.com", 25),
    new(3, "Bob", "b@email.com", 35)
};

IOrdenador<Persona> porNombre = new OrdenadorPorNombre<Persona>();
IOrdenador<Persona> porEdad = new OrdenadorPorEdad<Persona>();

Console.WriteLine("Orden por nombre: " + string.Join(", ", porNombre.Ordenar(personas).Select(p => p.Nombre)));
Console.WriteLine("Orden por edad: " + string.Join(", ", porEdad.Ordenar(personas).Select(p => p.Edad)));
Console.WriteLine();

// --- FACTORY: Crear objetos sin new directo ---
Console.WriteLine("--- FACTORY: Repositorio según configuración ---");
var repoMemory = PersonaRepositoryFactory.Crear("Memory");
var repoJson = PersonaRepositoryFactory.Crear("Json");

repoMemory.Guardar(new Persona(1, "Ana", "ana@email.com", 25));
Console.WriteLine($"Memory: {repoMemory.Obtener(1)?.Nombre}");
Console.WriteLine($"Json: {repoJson.ObtenerTodos().Count()} usuarios");
Console.WriteLine();

// --- DECORATOR: Añadir comportamiento sin modificar ---
Console.WriteLine("--- DECORATOR: Logging + Métricas + Retry ---");

// Empezamos con el repositorio base
var baseRepo = new InMemoryRepository<Persona>();

// Decoramos con Logging
IRepository<Persona> withLogging = new LoggingRepositoryDecorator<Persona>(baseRepo);

// Decoramos Logging + Metrics (encadenar decorators)
IRepository<Persona> withMetrics = new MetricsRepositoryDecorator<Persona>(withLogging);

// Usamos el repositorio decorado (el cliente no sabe qué hay debajo)
baseRepo.GuardarConId(1, new Persona(1, "Ana", "ana@email.com", 25));
var resultado = withMetrics.Obtener(1);
Console.WriteLine($"Resultado: {resultado?.Nombre}");
Console.WriteLine();

// --- ADAPTER: Convertir interfaz de API externa ---
Console.WriteLine("--- ADAPTER: API externa → nuestra interfaz ---");
var apiExterna = new ApiExternaUsuario();
var adapter = new ApiExternaAdapter(apiExterna);
var usuarioService = new UsuarioService(adapter);

usuarioService.Mostrar(1);
usuarioService.Mostrar(999); // No encontrado
Console.WriteLine();

// --- BUILDER: Construir objetos complejos paso a paso ---
Console.WriteLine("--- BUILDER: Configuración de servidor ---");

var config = new ServidorConfigBuilder()
    .ConHost("miapi.com")
    .ConPuerto(443)
    .ConHttps()
    .ConTimeout(60)
    .ConMaxConexiones(500)
    .ConDirectorioLog("/var/logs")
    .ConCors("https://miapp.com", "https://www.miapp.com")
    .Build();

Console.WriteLine($"Configuración: {config}");
Console.WriteLine();

// ============================================================
// RESUMEN
// ============================================================
Console.WriteLine("=== RESUMEN ===");
Console.WriteLine("SOLID:");
Console.WriteLine("  S (SRP):    Cada clase = 1 responsabilidad");
Console.WriteLine("  O (OCP):    Abierto a extensión, cerrado a modificación");
Console.WriteLine("  L (LSP):    Subtipos sustituibles por tipos base");
Console.WriteLine("  I (ISP):    Interfaces pequeñas y específicas");
Console.WriteLine("  D (DIP):    Depender de abstracciones, no de implementaciones");
Console.WriteLine();
Console.WriteLine("Patrones:");
Console.WriteLine("  Strategy:   Intercambiar algoritmos en runtime");
Console.WriteLine("  Factory:    Crear objetos sin new directo");
Console.WriteLine("  Decorator:  Añadir comportamiento sin modificar");
Console.WriteLine("  Adapter:    Convertir interfaz incompatible");
Console.WriteLine("  Builder:    Construir objetos complejos paso a paso");
