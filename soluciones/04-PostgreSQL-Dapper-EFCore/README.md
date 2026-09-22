# Ejemplo 03: PostgreSQL con Dapper y EF Core

## Descripción

Este ejemplo muestra cómo realizar operaciones CRUD en PostgreSQL usando dos enfoques diferentes:

- **Dapper**: Micro ORM ligero, ejecuta SQL directo
- **Entity Framework Core**: ORM completo con change tracking y migraciones

## Conceptos Clave

### Dapper vs EF Core

| Característica | Dapper | EF Core |
|----------------|--------|---------|
| **Tamaño** | ~15KB | ~5MB |
| **Velocidad** | Más rápido | Más lento |
| **Funciones** | Solo mapeo ORM | CRUD, migraciones, tracking |
| **SQL** | Manual | Generado automáticamente |
| **Control** | Total sobre la consulta | Abstraído |

### Cuándo usar cada uno

- **Dapper**: Consultas optimizadas, reports, alta performance
- **EF Core**: CRUD estándar, prototipado rápido, proyectos medianos

### Docker Compose

Levanta PostgreSQL 16 con un volumen persistente y un script de inicialización:

```bash
docker-compose up -d
# o con Podman: podman-compose up -d
```

### Inyección de Dependencias

Ambos repositorios implementan la misma interfaz `IProductoRepository`. Se pueden intercambiar fácilmente:

```csharp
// Con Dapper
services.AddSingleton<IProductoRepository>(new ProductoDapperRepository(connectionString));

// Con EF Core
services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
services.AddScoped<IProductoRepository, ProductoEfRepository>();
```

## Prerrequisitos

1. Docker y Docker Compose (o Podman y Podman Compose) instalados
2. .NET 10 SDK

## Ejecución

```bash
# 1. Levantar PostgreSQL
docker-compose up -d
# o con Podman: podman-compose up -d

# 2. Ejecutar con EF Core (por defecto)
dotnet run

# 3. Ejecutar con Dapper
dotnet run -- dapper

# 4. Parar PostgreSQL
docker-compose down
# o con Podman: podman-compose down
```

## Estructura

```
03-PostgreSQL-Dapper-EFCore/
├── 03-PostgreSQL-Dapper-EFCore.slnx
├── 03-PostgreSQL-Dapper-EFCore/
│   ├── 03-PostgreSQL-Dapper-EFCore.csproj
│   ├── Program.cs
│   ├── Models/
│   │   └── Producto.cs
│   ├── Entity/
│   │   └── ProductoEntity.cs
│   ├── Repositories/
│   │   ├── Base/
│   │   │   └── IProductoRepository.cs
│   │   ├── Dapper/
│   │   │   └── ProductoDapperRepository.cs
│   │   └── EfCore/
│   │       ├── AppDbContext.cs
│   │       └── ProductoEfRepository.cs
│   ├── docker-compose.yml
│   └── init.sql
└── README.md
```

## Paquetes NuGet

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `Dapper` | 2.1.66 | Micro ORM ligero |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.2 | EF Core para PostgreSQL |
| `Microsoft.EntityFrameworkCore.Sqlite` | 9.0.2 | EF Core (referencia) |
| `Microsoft.Extensions.DependencyInjection` | 9.0.0 | Inyección de dependencias |

## Referencias

- [Dapper (GitHub)](https://github.com/DapperLib/Dapper)
- [Entity Framework Core (Microsoft)](https://learn.microsoft.com/es-es/ef/core/)
- [Npgsql (PostgreSQL para .NET)](https://www.npgsql.org/)
- [Docker Compose (Documentación)](https://docs.docker.com/compose/)
