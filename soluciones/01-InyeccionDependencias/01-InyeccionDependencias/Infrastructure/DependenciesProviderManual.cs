using Microsoft.Extensions.DependencyInjection;
using ProductoApp.Repositories.Base;
using ProductoApp.Repositories.Memory;
using ProductoApp.Services;
using ProductoApp.Services.Base;

namespace ProductoApp.Infrastructure;

/// <summary>
/// Configuración manual de Inyección de Dependencias (sin Scrutor).
/// </summary>
public static class DependenciesProviderManual
{
    /// <summary>
    /// Construye el proveedor de servicios con registros manuales.
    /// </summary>
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Registrar repositorios
        services.AddTransient<IProductoRepository, ProductoMemoryRepository>();

        // Registrar servicios
        services.AddScoped<IProductoService, ProductoService>();

        return services.BuildServiceProvider();
    }
}
