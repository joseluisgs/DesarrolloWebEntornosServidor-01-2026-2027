# Práctica 5: Análisis de Accidentes de Madrid con LINQ y DataFrames

- [Práctica 5: Análisis de Accidentes de Madrid con LINQ y DataFrames](#práctica-5-análisis-de-accidentes-de-madrid-con-linq-y-dataframes)
  - [Objetivo](#objetivo)
  - [Descripción](#descripción)
  - [Estructura de Datos](#estructura-de-datos)
  - [Operaciones Requeridas](#operaciones-requeridas)
  - [Tecnologías](#tecnologías)
  - [Estructura de Proyecto](#estructura-de-proyecto)

---

## Objetivo

Procesar un fichero CSV con datos de accidentes de tráfico en Madrid y realizar consultas avanzadas usando **LINQ** y **DataFrames** (`Microsoft.Data.Analysis`). El objetivo es practicar las operaciones de LINQ sobre datos reales y comparar el enfoque de colecciones con el de DataFrames.

> *"Los datos son el nuevo petróleo"* — Clive Humby

---

## Descripción

Dado el fichero `2025_Accidentalidad.csv` del directorio `data`, debemos procesarlo y realizar consultas como si de una base de datos se tratara.

El proyecto debe:
1. Leer el CSV y mapearlo a objetos de dominio
2. Realizar 22 consultas LINQ sobre los datos
3. Comparar el enfoque LINQ (colecciones) con el enfoque DataFrame
4. Mostrar estadísticas por distrito, sexo, tipo de accidente, etc.

---

## Estructura de Datos

Un **Accidente** tiene:

```csharp
public record Accidente(
    string NumExpediente,
    DateTime Fecha,
    TimeSpan Hora,
    string Localizacion,
    int Numero,
    int CodDistrito,
    string Distrito,
    string TipoAccidente,
    string EstadoMeteorologico,
    string TipoVehiculo,
    TipoPersona TipoPersona,
    string RangoEdad,
    Sexo Sexo,
    string CodLesividad,
    string Lesividad,
    bool PositivoAlcohol,
    bool PositivoDroga
);

public enum TipoPersona { Conductor, Pasajero, Peatón }
public enum Sexo { Hombre, Mujer, NoAsignado }
```

---

## Operaciones Requeridas

### LINQ (colecciones)

1. 5 primeros accidentes
2. Accidentes con alcohol o drogas
3. Positivos alcohol Y drogas
4. Por sexo
5. Por meses
6. Mes con más accidentes
7. Por tipo de vehículo
8. Accidentes en calle Leganés
9. Por distrito (ASC)
10. Accidentes en USERA
11. Stats por distrito (Max/Min/Avg)
12. Por distrito (DESC)
13. Fin de semana + noche + alcohol
14. Por lesividad
15. Fallecidos
16. Fallecidos + alcohol/drogas
17. Por meteorología
18. Granizo por distrito
19. Alcohol/Drogas/Nada
20. Distrito más alcohol
21. Distrito más drogas
22. Distrito más alcohol+drogas

### DataFrame (Microsoft.Data.Analysis)

Además, implementar al menos 5 de las mismas consultas usando DataFrame para comparar:

```csharp
using Microsoft.Data.Analysis;

// Leer CSV
var df = DataFrame.LoadCsv("data/2025_Accidentalidad.csv");

// Filtrar: accidentes con alcohol
var conAlcohol = df.Filter(df.Columns["PositivoAlcohol"].Cast<bool>().EqualTo(true));

// Agrupar: por distrito
var porDistrito = df.GroupBy("Distrito");
foreach (var grupo in porDistrito)
{
    Console.WriteLine($"{grupo.Key}: {grupo.RowCount} accidentes");
}

// Estadísticas
var total = df.Rows.Count;
var conDroga = df.Filter(df.Columns["PositivoDroga"].Cast<bool>().EqualTo(true)).Rows.Count;
```

---

## Tecnologías

| Tecnología | Para qué | Paquete NuGet |
|------------|----------|---------------|
| **Microsoft.Data.Analysis** | DataFrames para datos tabulares | `Microsoft.Data.Analysis` |
| **CsvHelper** | Leer CSV de forma robusta | `CsvHelper` |
| **LINQ** | Consultas sobre colecciones | `System.Linq` |
| **C# 14** | Primary constructors, top-level statements | — |

---

## Estructura de Proyecto

```
AccidentesMadrid/
├── Program.cs
├── AccidentesMadrid.csproj
├── data/
│   └── 2025_Accidentalidad.csv
├── Models/
│   ├── Accidente.cs
│   ├── Sexo.cs
│   └── TipoPersona.cs
├── Mappers/
│   └── AccidenteMapper.cs
├── Repositories/
│   └── AccidentesRepository.cs
├── Services/
│   └── AccidentesAnalyzer.cs
├── DataFrame/
│   └── AccidentesDataFrameAnalyzer.cs
└── README.md
```

### Ejemplo de uso combinado

```csharp
// LINQ: objetos de dominio
var accidentes = repository.GetAll();
var conAlcoholLinq = accidentes.Where(a => a.PositivoAlcohol).ToList();

// DataFrame: datos tabulares
var df = DataFrame.LoadCsv("data/2025_Accidentalidad.csv");
var conAlcoholDf = df.Filter(df.Columns["PositivoAlcohol"].Cast<bool>().EqualTo(true));

Console.WriteLine($"LINQ: {conAlcoholLinq.Count} accidentes con alcohol");
Console.WriteLine($"DataFrame: {conAlcoholDf.Rows.Count} accidentes con alcohol");
```

---

## Entrega

Sube el proyecto a tu repositorio GitHub con el nombre `AccidentesMadrid`.
