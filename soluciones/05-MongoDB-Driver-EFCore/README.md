# Ejemplo 05: MongoDB con Driver y EF Core

## Descripción

Este ejemplo muestra cómo realizar operaciones CRUD en MongoDB usando dos enfoques diferentes:

- **Driver nativo de MongoDB**: Control total, API específica de MongoDB
- **Entity Framework Core**: ORM familiar, intercambiable con otros proveedores

## Conceptos Clave

### MongoDB vs PostgreSQL

| Característica | MongoDB | PostgreSQL |
|----------------|---------|------------|
| **Tipo** | NoSQL (documentos) | SQL (relacional) |
| **Schema** | Flexible (schemaless) | Estricto |
| **Escalado** | Horizontal | Vertical |
| **Datos** | JSON/BSON | Tablas y filas |
| **Uso típico** | Big Data, IoT, CMS | Transacciones, ERP |

### Driver Nativo vs EF Core

| Característica | Driver MongoDB | EF Core MongoDB |
|----------------|----------------|-----------------|
| **Tamaño** | ~2MB | ~5MB |
| **API** | MongoDB específica | API estándar EF Core |
| **Funciones** | Aggregation pipeline | LINQ, change tracking |
| **Migraciones** | No | Sí (limitado) |
| **Control** | Total | Abstraído |

### Docker Compose

Levanta MongoDB 7 con Mongo Express (interfaz web):

```bash
docker-compose up -d
# o con Podman: podman-compose up -d
```

Acceso a Mongo Express: `http://localhost:8081`
- Usuario: `admin`
- Contraseña: `admin`

### ObjectId de MongoDB

MongoDB usa `ObjectId` como identificador único. Es un valor de 12 bytes que contiene:
- Timestamp (4 bytes)
- Machine ID (5 bytes)
- Process ID (2 bytes)
- Counter (3 bytes)

```csharp
[BsonId]
[BsonRepresentation(BsonType.ObjectId)]
public string Id { get; set; }
```

## Prerrequisitos

1. Docker y Docker Compose (o Podman y Podman Compose) instalados
2. .NET 10 SDK

## Ejecución

```bash
# 1. Levantar MongoDB y Mongo Express
docker-compose up -d
# o con Podman: podman-compose up -d

# 2. Ejecutar con Driver Nativo (por defecto)
dotnet run

# 3. Ejecutar con EF Core
dotnet run -- efcore

# 4. Parar MongoDB
docker-compose down
# o con Podman: podman-compose down
```

## Estructura

```
05-MongoDB-Driver-EFCore/
├── 05-MongoDB-Driver-EFCore.slnx
├── 05-MongoDB-Driver-EFCore/
│   ├── 05-MongoDB-Driver-EFCore.csproj
│   ├── Program.cs
│   ├── Models/
│   │   └── Producto.cs
│   ├── Repositories/
│   │   ├── Base/
│   │   │   └── IProductoRepository.cs
│   │   ├── MongoDriver/
│   │   │   └── ProductoMongoDriverRepository.cs
│   │   └── MongoEfCore/
│   │       ├── MongoDbContext.cs
│   │       └── ProductoMongoEfRepository.cs
│   └── docker-compose.yml
└── README.md
```

## Paquetes NuGet

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `MongoDB.Driver` | 3.1.0 | Driver nativo de MongoDB |
| `Microsoft.EntityFrameworkCore.MongoDB` | 9.0.2 | EF Core para MongoDB |
| `Microsoft.Extensions.DependencyInjection` | 9.0.0 | Inyección de dependencias |

## Casos de Uso

- **Driver Nativo**: Apps que necesitan aggregation pipelines, change streams
- **EF Core**: Apps que ya usan EF Core y quieren cambiar de BD fácilmente

## Referencias

- [MongoDB Driver .NET (Documentación)](https://www.mongodb.com/docs/drivers/csharp/)
- [EF Core con MongoDB (Microsoft)](https://learn.microsoft.com/es-es/ef/core/providers/mongodb/)
- [MongoDB University (Cursos gratuitos)](https://university.mongodb.com/)
