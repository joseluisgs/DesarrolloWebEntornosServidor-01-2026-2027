// ============================================================
// Ejemplo 12: Docker Hello World
// Console + Minimal API + Test + Docker multi-etapa
// ============================================================

// Mensaje en consola (se ve al arrancar el contenedor)
Console.WriteLine("=== Ejemplo 12: Docker Hello World ===");
Console.WriteLine("Hola desde Docker (consola)");

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Minimal API: responde por HTTP
app.MapGet("/", () => "Hola desde Docker (web)");

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.Now }));

app.Run();
