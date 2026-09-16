using Microsoft.Extensions.DependencyInjection;
using ProductoApp.Interfaces;
using Scrutor;

namespace ProductoApp.Infrastructure;

/// <summary>
/// Configuración de Inyección de Dependencias con Scrutor (escaneo automático).
/// </summary>
/// <remarks>
/// Scrutor escanea el ensamblado y registra automáticamente las clases
/// que implementen las interfaces de marcador de ciclo de vida:
///   - ITransientService → Transient
///   - IScopedService    → Scoped
///   - ISingletonService  → Singleton
///
/// REQUISITO: Cada clase de servicio/repositorio DEBE implementar:
///   1. Su interfaz de negocio (IProductoService, IProductoRepository)
///   2. Una interfaz de marcador de ciclo de vida (ITransientService, etc.)
///
/// Si una clase no implementa ninguna de estas interfaces, Scrutor la ignora.
/// </remarks>
public static class DependenciesProviderScrutor
{
    /// <summary>
    /// Construye el proveedor de servicios con escaneo automático.
    /// </summary>
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Scrutor escanea todo el ensamblado y registra automáticamente:
        // - Clases que implementen ITransientService → Transient
        // - Clases que implementen IScopedService → Scoped
        // - Clases que implementen ISingletonService → Singleton
        //
        // Cada clase DEBE tener una interfaz para ser detectada.
        // Ejemplo: ProductoService implementa IProductoService + IScopedService
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
                .AddClasses(classes => classes.AssignableTo<ITransientService>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                .AddClasses(classes => classes.AssignableTo<IScopedService>())
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
                .AddClasses(classes => classes.AssignableTo<ISingletonService>())
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
        );
        
        

        return services.BuildServiceProvider();
    }
}
