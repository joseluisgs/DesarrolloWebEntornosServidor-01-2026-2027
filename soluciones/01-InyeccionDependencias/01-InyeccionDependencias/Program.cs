using Microsoft.Extensions.DependencyInjection;
using ProductoApp.Infrastructure;
using ProductoApp.Repositories.Base;
using ProductoApp.Services.Base;

Console.WriteLine("=== Ejemplo de Inyección de Dependencias en .NET ===");
Console.WriteLine();

// ============================================================
// ENFOQUE 1: REGISTRO MANUAL (sin dependencias externas)
// ============================================================
// Cada servicio se registra uno por uno. Explícito pero verboso.
// Útil cuando tienes pocos servicios o necesitas control total.
Console.WriteLine("--- Enfoque 1: Registro Manual ---");
var providerManual = DependenciesProviderManual.BuildServiceProvider();

using (var scope = providerManual.CreateScope())
{
    var serviceManual = scope.ServiceProvider.GetRequiredService<IProductoService>();
    var productos = serviceManual.GetAll();
    Console.WriteLine($"Manual: {productos.Count()} productos encontrados");
}
Console.WriteLine();

// ============================================================
// ENFOQUE 2: SCRUTOR (escaneo automático)
// ============================================================
// Scrutor escanea el ensamblado y registra automáticamente
// todas las clases que implementen una interfaz de servicio.
//
// REQUISITO: Cada clase debe implementar una interfaz
// (IProductoRepository, IProductoService, etc.)
// y marcarse con una interfaz de ciclo de vida:
//   - ITransientService → Transient (nueva instancia cada vez)
//   - IScopedService    → Scoped (una por petición HTTP)
//   - ISingletonService  → Singleton (una sola para toda la app)
//
// Ventaja: no hay que escribir cada registro manualmente.
// Si añades un nuevo repositorio, Scrutor lo detecta solo.
Console.WriteLine("--- Enfoque 2: Scrutor (escaneo automático) ---");
var providerScrutor = DependenciesProviderScrutor.BuildServiceProvider();

using (var scope = providerScrutor.CreateScope())
{
    var serviceScrutor = scope.ServiceProvider.GetRequiredService<IProductoService>();
    var productos = serviceScrutor.GetAll();
    Console.WriteLine($"Scrutor: {productos.Count()} productos encontrados");
}
Console.WriteLine();

Console.WriteLine("=== Configuración DI completada ===");
Console.WriteLine();
Console.WriteLine("RESUMEN:");
Console.WriteLine("  Manual:  Registro explícito, control total, verboso");
Console.WriteLine("  Scrutor: Escaneo automático, menos código, requiere interfaces");
