# Ejemplo 03: Ficheros JSON y CSV

## Descripción

Este ejemplo demuestra cómo leer y escribir ficheros en formatos JSON y CSV usando C#. Son dos de los formatos más utilizados para intercambio de datos.

## Conceptos Clave

### JSON (JavaScript Object Notation)

Formato ligero de intercambio de datos. En C# usamos `System.Text.Json` (viene con .NET):

```csharp
// Leer JSON
var json = await File.ReadAllTextAsync("datos.json");
var productos = JsonSerializer.Deserialize<List<Producto>>(json);

// Escribir JSON
var opciones = new JsonSerializerOptions { WriteIndented = true };
var jsonExport = JsonSerializer.Serialize(productos, opciones);
await File.WriteAllTextAsync("export.json", jsonExport);
```

### CSV (Comma-Separated Values)

Formato tabular simple. Usamos la librería `CsvHelper` para leer/escribir:

```csharp
// Leer CSV
using var reader = new StreamReader("datos.csv");
using var csv = new CsvReader(reader, config);
var productos = csv.GetRecords<Producto>().ToList();

// Escribir CSV
using var writer = new StreamWriter("export.csv");
using var csv = new CsvWriter(writer, config);
csv.WriteRecords(productos);
```

### Record como modelo

Usamos `record` para modelos inmutables, ideales para datos:

```csharp
public record Producto(int Id, string Nombre, decimal Precio, string Categoria);
```

## Estructura

```
03-FicherosJsonCsv/
├── 03-FicherosJsonCsv.slnx
├── 03-FicherosJsonCsv/
│   ├── 03-FicherosJsonCsv.csproj
│   ├── Program.cs
│   ├── Models/
│   │   └── Producto.cs
│   └── data/
│       ├── productos.json
│       └── productos.csv
└── README.md
```

## Ejecución

```bash
dotnet run
```

Los ficheros de salida se crearán en la carpeta `output/`.

## Paquetes NuGet

| Paquete | Versión | Descripción |
|---------|---------|-------------|
| `CsvHelper` | 33.0.1 | Lectura/escritura de CSV |

## Comparativa: JSON vs CSV

| Característica | JSON | CSV |
|----------------|------|-----|
| **Legibilidad** | Alta | Media |
| **Jerarquías** | Sí | No |
| **Tamaño** | Mayor | Menor |
| **Uso típico** | APIs web | Exportaciones Excel |
| **Tipos de datos** | Soportados | Todo es texto |

## Referencias

- [System.Text.Json (Microsoft)](https://learn.microsoft.com/es-es/dotnet/system.text.json)
- [CsvHelper (Documentación)](https://joshclose.github.io/CsvHelper/)
- [Ficheros y flujos en C# (Microsoft)](https://learn.microsoft.com/es-es/dotnet/standard/io)
