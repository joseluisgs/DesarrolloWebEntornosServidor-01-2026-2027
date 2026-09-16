# Ejemplo 01: Inyección de Dependencias en .NET

## Descripción

Este ejemplo demuestra el uso de **Inyección de Dependencias (DI)** en .NET, mostrando dos enfoques:
- **DI Manual**: registro explícito de cada servicio
- **DI con Scrutor**: escaneo automático usando interfaces marcadoras

## Conceptos Clave

### ¿Qué es la Inyección de Dependencias?

La Inyección de Dependencias es un patrón de diseño donde un objeto recibe sus dependencias de fuentes externas en lugar de crearlas él mismo. Esto promueve:

- **Desacoplamiento**: las clases no dependen de implementaciones concretas
- **Testabilidad**: fácil de mockear dependencias
- **Mantenibilidad**: cambiar implementaciones sin modificar el código cliente

### Duración de los Servicios

| Tipo | Descripción | Ejemplo |
|------|-------------|---------|
| **Transient** | Nueva instancia cada vez que se solicita | Repositorios, servicios stateless |
| **Scoped** | Una instancia por petición HTTP | Servicios de negocio |
| **Singleton** | Una única instancia reutilizada | Caché, configuración |

### Interfaces Marcadoras

Las interfaces marcadoras son interfaces vacías que indican la duración deseada del servicio:

```csharp
public interface ITransientService { }
public interface IScopedService { }
public interface ISingletonService { }
```

## Estructura del Proyecto

```
01-InyeccionDependencias/
├── 01-InyeccionDependencias.slnx
├── 01-InyeccionDependencias/
│   ├── Program.cs                    # Punto de entrada (Top Level Statements)
│   ├── Models/
│   │   └── Producto.cs               # Modelo de dominio (record)
│   ├── Repositories/
│   │   ├── Base/
│   │   │   └── IProductoRepository.cs
│   │   └── Memory/
│   │       └── ProductoMemoryRepository.cs
│   ├── Services/
│   │   ├── Base/
│   │   │   └── IProductoService.cs
│   │   └── ProductoService.cs        # Primary constructor
│   ├── Interfaces/
│   │   ├── ITransientService.cs
│   │   ├── IScopedService.cs
│   │   └── ISingletonService.cs
│   └── Infrastructure/
│       ├── DependenciesProviderManual.cs
│       └── DependenciesProviderScrutor.cs
└── 01-InyeccionDependencias.Tests/
    └── Services/
        └── ProductoServiceTests.cs
```

## Comparativa: Manual vs Scrutor

| Aspecto | DI Manual | DI con Scrutor |
|---------|-----------|----------------|
| **Registro** | Explícito uno por uno | Automático por escaneo |
| **Mantenimiento** | Alta carga con muchos servicios | Bajo mantenimiento |
| **Legibilidad** | Código más largo | Código más limpio |
| **Flexibilidad** | Control total | Convenciones sobre configuración |
| **Uso recomendado** | Proyectos pequeños, pocos servicios | Proyectos grandes, muchos servicios |

## Ejecución

```bash
# Compilar y ejecutar
dotnet run

# Ejecutar tests
dotnet test
```

## Ejemplo Real

**Netflix** usa un sistema similar de Inyección de Dependencias para gestionar sus múltiples microservicios:
- Cada servicio (recomendaciones, pagos, notificaciones) se registra con una duración específica
- Los servicios stateless usan **Transient**
- Los servicios de negocio usan **Scoped**
- La configuración global usa **Singleton**

## Enlaces

- [Documentación oficial de DI en .NET](https://learn.microsoft.com/es-es/dotnet/core/extensions/dependency-injection)
- [Scrutor en GitHub](https://github.com/khellang/Scrutor)
- [Vídeo: Inyección de Dependencias en .NET](https://www.youtube.com/watch?v=8z)
