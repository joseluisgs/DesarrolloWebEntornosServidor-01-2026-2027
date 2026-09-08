# Práctica 5: Análisis de Accidentes de Madrid con LINQ, PLINQ y DataFrames

- [Práctica 5: Análisis de Accidentes de Madrid con LINQ, PLINQ y DataFrames](#práctica-5-análisis-de-accidentes-de-madrid-con-linq-plinq-y-dataframes)
  - [Objetivo](#objetivo)
  - [Descripción](#descripción)
  - [Fichero de Datos](#fichero-de-datos)
  - [Estructura de Datos](#estructura-de-datos)
  - [Operaciones Requeridas](#operaciones-requeridas)
  - [Tecnologías](#tecnologías)
  - [Estructura de Proyecto](#estructura-de-proyecto)
  - [Comparativa de Tiempos](#comparativa-de-tiempos)

---

## Objetivo

Procesar un fichero CSV con datos reales de accidentes de tráfico en Madrid y realizar consultas avanzadas usando **LINQ**, **PLINQ** y **DataFrames** (`Microsoft.Data.Analysis`). El objetivo es practicar las operaciones de LINQ sobre datos reales, entender cuándo usar PLINQ, y comparar el enfoque de colecciones con el de DataFrames.

> *"Los datos son el nuevo petróleo"* — Clive Humby

---

## Descripción

Dado el fichero `2025_Accidentalidad.csv` del directorio `data`, debemos procesarlo y realizar consultas como si de una base de datos se tratara.

El proyecto debe:
1. Leer el CSV y mapearlo a objetos de dominio (usando CsvHelper)
2. Realizar 23 consultas LINQ sobre los datos
3. Incluir al menos 1 consulta con PLINQ (`AsParallel`)
4. Demostrar `GroupBy + ToDictionary` vs `GroupBy + Select + ToList`
5. Realizar al menos 5 consultas equivalentes con DataFrames
6. Comparar tiempos de ejecución entre LINQ y DataFrames

---

## Fichero de Datos

El fichero CSV se descarga de los **datos abiertos del Ayuntamiento de Madrid**:

📥 **URL de descarga:**
```
https://datos.madrid.es/dataset/300228-0-accidentes-trafico-detalle/information
```

En esa página encontrarás los ficheros de varios años. Descarga el de **2025** y colócalo en la carpeta `data/` de tu proyecto.

> ⚠️ **Nota:** El fichero es grande (~9 MB, ~46.000 registros).

**Cabeceras del CSV (separador `;`):**
```
num_expediente;fecha;hora;localizacion;numero;cod_distrito;distrito;tipo_accidente;
estado_meteorológico;tipo_vehiculo;tipo_persona;rango_edad;sexo;cod_lesividad;
lesividad;coordenada_x_utm;coordenada_y_utm;positiva_alcohol;positiva_droga
```

---

## Estructura de Datos

Deduce la estructura del modelo a partir de las cabeceras del CSV. Ten en cuenta que:

- `numero` puede contener valores no numéricos
- `positiva_alcohol` y `positiva_droga` usan "S"/"N" en el CSV
- Algunos campos pueden estar vacíos

---

## Operaciones Requeridas

### LINQ (23 consultas)

1. Total de accidentes
2. Accidentes por distrito (top 5)
3. Accidentes por tipo
4. Accidentes por estado meteorológico
5. Accidentes por sexo
6. Accidentes por rango de edad
7. Positivos en alcohol
8. Positivos en drogas
9. Accidentes por día de la semana
10. Accidentes por mes
11. Hora con más accidentes
12. Lesiones más frecuentes
13. Tipo de vehículo más implicado
14. Accidentes con peatones
15. Proporción hombre/mujer
16. Distritos con más peatones
17. Fin de semana vs entre semana
18. Media de accidentes por día
19. Accidentes con alcohol + droga
20. Rangos de edad más vulnerables (peatones)
21. Distritos con más positivos en alcohol

### PLINQ (1 consulta)

22. Accidentes por hora usando `AsParallel()` — justificar por qué esta consulta y no otras

### GroupBy eficiente (1 consulta)

23. Demostrar la diferencia entre `GroupBy + ToDictionary` y `GroupBy + Select + ToList`

### DataFrame (Microsoft.Data.Analysis)

Implementar las mismas 23 consultas usando DataFrame para comparar tiempos con LINQ

---

## Tecnologías

| Tecnología | Para qué | Paquete NuGet |
|------------|----------|---------------|
| **Microsoft.Data.Analysis** | DataFrames para datos tabulares | `Microsoft.Data.Analysis` |
| **CsvHelper** | Leer CSV de forma robusta | `CsvHelper` |
| **LINQ** | Consultas sobre colecciones | `System.Linq` |
| **PLINQ** | Paralelización de consultas | `System.Linq` (`.AsParallel()`) |
| **C# 14** | Primary constructors, top-level statements | — |

---

## Estructura de Proyecto

```
AccidentesMadrid/
├── Program.cs
├── AccidentesMadrid.csproj
├── data/
│   └── 2025_Accidentalidad.csv      ← Descargado de datos.madrid.es
├── Models/
│   ├── Accidente.cs
│   ├── Sexo.cs
│   └── TipoPersona.cs
├── Mappers/
│   └── AccidenteMapper.cs
├── Repositories/
│   └── AccidentesRepository.cs
├── Services/
│   ├── IAccidentesAnalyzer.cs
│   ├── AccidentesLinqAnalyzer.cs     ← LINQ + PLINQ + ToDictionary
│   └── AccidentesDataFrameAnalyzer.cs ← DataFrames
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
└── README.md
```

### Ejemplo de uso combinado

El proyecto debe combinar las tres aproximaciones (LINQ, PLINQ, DataFrame) y mostrar una comparativa de tiempos al final.

---

## Comparativa de Tiempos

El ejemplo incluye una comparativa automática de tiempos:

```
═══════════════════════════════════════════════════
  COMPARATIVA DE TIEMPOS
═══════════════════════════════════════════════════
  LINQ / PLINQ:       171 ms
  DataFrames:         359 ms
  Ratio LINQ/DF:   0,48x

LINQ es más rápido para este volumen de datos.
```

> 💡 **Consejo:** Con 46K registros, LINQ es ~2x más rápido que DataFrames. DataFrames tiene overhead por crear la estructura tabular. Con 100K+ registros y análisis estadístico pesado, DataFrames podría ser mejor.

---

## Entrega

Sube el proyecto a tu repositorio GitHub con el nombre `AccidentesMadrid`.

Incluye:
1. Código fuente completo
2. Fichero CSV en `data/`
3. Dockerfile y docker-compose.yml
4. README con instrucciones de uso
