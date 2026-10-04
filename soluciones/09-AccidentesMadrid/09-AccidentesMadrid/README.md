# Ejercicio 09 — Accidentes de Madrid: LINQ, PLINQ y DataFrames

## Descripción

Este ejemplo procesa un fichero CSV con datos de accidentes de tráfico en Madrid utilizando tres enfoques diferentes:

- **LINQ** (Language Integrated Query) para consultas en memoria
- **PLINQ** (Parallel LINQ) para consultas paralelas
- **DataFrames** con `Microsoft.Data.Analysis` para análisis tabular

Además, las mismas 30 consultas se ejecutan con **dos motores de paralelismo** —`Task.WhenAll` (async) y `Parallel.For` (bloqueante)— y con **8 granularidades** distintas (de 1 a 30 tareas) para medir el punto óptimo.

## Qué mide esta solución

La app ejecuta 5 fases y publica sus tiempos en el [README de la solución](../README.md):

| Fase | Qué hace | Motor |
|------|----------|-------|
| 1 | Lectura de los 3 CSV | `Task.WhenAll` (E/S) |
| 2 | 30 consultas LINQ (×30 + 6 lotes) | `Task.WhenAll` |
| 3 | 30 consultas DataFrame (×30 + 6 lotes) | `Task.WhenAll` |
| 4 | Las mismas consultas LINQ (×30 + 6 lotes) | `Parallel.For` |
| 5 | Las mismas consultas DataFrame (×30 + 6 lotes) | `Parallel.For` |

Variantes de granularidad (las 30 consultas deben dividirse en partes iguales): secuencial (1 × 30), 2 × 15, 3 × 10, 5 × 6, 6 × 5, 10 × 3, 15 × 2 y 30 × 1.

> 📝 **Nota:** Las mediciones reales (tabla completa, gráficas de valle y análisis) están en el README de la solución. **No te aprendas los números: ejecuta y mide en tu máquina.**

## Conceptos clave

### LINQ (language integrated query)

LINQ es una extensión de C# que permite escribir consultas sobre colecciones de datos de forma integrada en el lenguaje. Funciona como "SQL pero dentro de C#".

```csharp
var accidentesPorDistrito = accidentes
    .GroupBy(a => a.Distrito)
    .OrderByDescending(g => g.Count())
    .Select(g => new { Distrito = g.Key, Cantidad = g.Count() });
```

**Ventajas:**
- Sintaxis limpia y legible
- Tipado estático (el compilador detecta errores)
- Funciona con cualquier colección `IEnumerable<T>`
- Lazy evaluation (solo ejecuta cuando se itera)

**Operadores habituales:**
| Operador | Descripción | Ejemplo |
|----------|-------------|---------|
| `Where` | Filtra elementos | `.Where(a => a.PositivoAlcohol)` |
| `Select` | Proyecta/transforma | `.Select(a => a.Distrito)` |
| `GroupBy` | Agrupa elementos | `.GroupBy(a => a.TipoAccidente)` |
| `OrderBy` | Ordena | `.OrderByDescending(g => g.Count())` |
| `Count` | Cuenta elementos | `.Count(a => a.Sexo == Sexo.Hombre)` |
| `Distinct` | Valores únicos | `.Select(a => a.Fecha.Date).Distinct()` |
| `Take` | Primeros N elementos | `.Take(5)` |

📌 Ejemplo real: **Netflix** usa consultas similares a LINQ internamente para filtrar y recomendar contenido según tus preferencias.

### PLINQ (parallel LINQ)

PLINQ es la versión paralela de LINQ. Con solo añadir `.AsParallel()`, las consultas se ejecutan en múltiples hilos del procesador.

```csharp
var porHora = accidentes
    .AsParallel()
    .GroupBy(a => a.Hora.Hours)
    .Select(g => new { Hora = g.Key, Cantidad = g.Count() });
```

**Cuándo usar PLINQ:**
- Colecciones grandes (>10.000 elementos)
- Consultas que no dependen del orden
- Operaciones pesadas (cálculos complejos)
- Procesamiento de ficheros grandes

**Precauciones:**
- No usar con colecciones pequeñas (la sobrecarga paralela supera el beneficio)
- Cuidado con las condiciones de carrera (race conditions)
- El orden de los resultados puede no ser el original

📌 Ejemplo real: **Glovo** usa procesamiento paralelo para calcular las rutas de reparto de miles de repartidores simultáneamente.

### DataFrames (Microsoft.Data.Analysis)

Un DataFrame es una estructura de datos tabular, similar a las de Python (pandas) o R. Permite trabajar con columnas de datos de forma eficiente.

```csharp
var df = new DataFrame(
    new StringDataFrameColumn("Distrito", distritos),
    new Int32DataFrameColumn("Accidentes", conteos)
);
```

**Ventajas:**
- Manejo eficiente de grandes volúmenes de datos
- Operaciones vectorizadas (más rápidas que bucles)
- Compatible con análisis de datos y machine learning
- Interoperabilidad con pandas (a través de .NET)

**Desventajas:**
- Menos intuitivo que LINQ para desarrolladores C#
- Requiere un paquete NuGet adicional
- Menos flexible para consultas complejas

📌 Ejemplo real: **Tesla** usa DataFrames para analizar millones de kilómetros de datos de conducción autónoma.

### CsvHelper

CsvHelper es una librería para leer y escribir ficheros CSV en C#. Es robusta, rápida y maneja casos complejos (comillas, comas dentro de campos, codificación, etc.).

```csharp
using var csv = new CsvReader(reader, config);
csv.Context.RegisterClassMap<AccidenteMapper>();
var records = csv.GetRecords<Accidente>().ToList();
```

**Características:**
- Mapeo de columnas a propiedades con `ClassMap`
- Manejo de errores y validación
- Soporte para diferentes separadores y codificaciones
- Conversión automática de tipos

📌 Ejemplo real: **Banco Santander** usa CsvHelper para procesar extractos bancarios y movimientos de clientes.

## Estructura del proyecto

```
09-AccidentesMadrid/
├── 09-AccidentesMadrid.slnx
├── 09-AccidentesMadrid/
│   ├── Program.cs                    # Fases 1-5 + resumen
│   ├── Models/
│   │   ├── Accidente.cs              # Modelo de dominio
│   │   ├── Sexo.cs                   # Enum de sexo
│   │   └── TipoPersona.cs            # Enum de tipo de persona
│   ├── Mappers/
│   │   └── AccidenteMapper.cs        # Mapeo CSV → Modelo
│   ├── Repositories/
│   │   ├── IAccidentesRepository.cs  # Interfaz del repositorio
│   │   └── AccidentesRepository.cs   # Lectura del CSV
│   ├── Services/
│   │   ├── IAccidentesAnalyzer.cs    # Contrato: secuencial, tareas, lotes, Parallel.For
│   │   ├── AccidentesLinqAnalyzer.cs # Consultas LINQ/PLINQ
│   │   └── AccidentesDataFrameAnalyzer.cs # Consultas con DataFrames
│   ├── data/
│   │   ├── 2024_Accidentalidad.csv   # Descargado de datos.madrid.es
│   │   ├── 2025_Accidentalidad.csv   # Descargado de datos.madrid.es
│   │   └── 2026_Accidentalidad.csv   # Descargado de datos.madrid.es
│   └── README.md
├── 09-AccidentesMadrid.Tests/
│   └── AccidenteLinqTests.cs         # Tests NUnit
└── README.md                         # Mediciones y análisis
```

## Paquetes NuGet

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `CsvHelper` | 33.0.1 | Lectura de ficheros CSV |
| `Microsoft.Data.Analysis` | 0.22.0 | DataFrames para análisis tabular |

## Ejecución

```bash
dotnet run

# Tests
dotnet test
```

## Comparativa de enfoques

> 📝 **Nota:** Esta tabla es **conceptual**. Las mediciones reales de esta solución (por enfoque, motor y granularidad) están en el [README de la solución](../README.md).

| Aspecto | LINQ | PLINQ | DataFrames |
|---------|------|-------|------------|
| **Velocidad (pocos datos)** | Rápido | Lento (overhead) | Medio |
| **Velocidad (muchos datos)** | Medio | Rápido | Muy rápido |
| **Legibilidad** | Excelente | Buena | Media |
| **Flexibilidad** | Alta | Alta | Media |
| **Uso de memoria** | Bajo | Medio | Alto |
| **Caso de uso ideal** | Consultas complejas | Procesamiento masivo | Análisis de datos |

## Conclusión

No existe un enfoque "mejor" en todos los casos. La elección depende del volumen de datos, la complejidad de las consultas y los requisitos de rendimiento:

- **Pocos datos, consultas simples** → LINQ
- **Muchos datos, consultas paralelizables** → PLINQ
- **Análisis tabular, grandes volúmenes** → DataFrames
