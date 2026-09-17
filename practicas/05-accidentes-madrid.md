# Práctica 5: Análisis de Accidentes de Madrid con LINQ, PLINQ y DataFrames

- [Práctica 5: Análisis de Accidentes de Madrid con LINQ, PLINQ y DataFrames](#práctica-5-análisis-de-accidentes-de-madrid-con-linq-plinq-y-dataframes)
  - [Objetivo](#objetivo)
  - [Descripción](#descripción)
  - [Fichero de Datos](#fichero-de-datos)
  - [Estructura de Datos](#estructura-de-datos)
  - [Operaciones Requeridas](#operaciones-requeridas)
  - [Justificación del Diseño](#justificación-del-diseño)
  - [Tecnologías](#tecnologías)
  - [Estructura de Proyecto](#estructura-de-proyecto)
  - [Comparativa de Tiempos](#comparativa-de-tiempos)

---

## Objetivo

Procesar ficheros CSV con datos reales de accidentes de tráfico en Madrid (2024, 2025 y 2026) y realizar consultas avanzadas usando **LINQ**, **PLINQ** y **DataFrames** (`Microsoft.Data.Analysis`). El objetivo es practicar:

1. Operaciones de LINQ sobre datos reales
2. Comparar el enfoque de colecciones con el de DataFrames
3. **Optimizar el rendimiento** del programa usando los recursos del sistema disponibles
4. **Justificar** las decisiones de diseño en todo momento

> *"Los datos son el nuevo petróleo"* — Clive Humby

---

## Descripción

Dado los ficheros CSV `2024_Accidentalidad.csv`, `2025_Accidentalidad.csv` y `2026_Accidentalidad.csv` del directorio `data`, debemos:

1. Leer los 3 ficheros CSV
2. Combinar los datos en una sola colección
3. Realizar **30 consultas LINQ** sobre el conjunto combinado
4. Realizar las **mismas 30 consultas** usando DataFrames
5. Medir y mostrar los tiempos de ejecución de cada operación
6. **Optimizar el rendimiento total del programa usando los recursos del sistema disponibles** (debes justificar tus decisiones)

---

## Fichero de Datos

Los ficheros CSV se descargan de los **datos abiertos del Ayuntamiento de Madrid**:

📥 **URL de descarga:**
```
https://datos.madrid.es/dataset/300228-0-accidentes-trafico-detalle/information
```

En esa página encontrarás los ficheros de varios años. Descarga los de **2024, 2025 y 2026** y colócalos en la carpeta `data/` de tu proyecto.

> ⚠️ **Nota:** Cada fichero es grande (~6-10 MB, ~30.000-51.000 registros). El total combinado puede superar los 100.000 registros.

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
- `fecha` tiene formato `dd/MM/yyyy` — necesitas extraer año, mes, día y día de la semana

---

## Operaciones Requeridas

### Lectura de Ficheros (1 punto)

Leer los 3 ficheros CSV del directorio `data/` y combinarlos en una sola colección.

### LINQ (30 consultas)

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
22. Accidentes por código de distrito
23. Accidentes por año
24. Evolución mensual por año
25. Distrito con más accidentes por año
26. Tendencia de alcohol por año
27. Comparativa fin de semana vs entre semana por año
28. Hora pico por año
29. Lesión más frecuente por año
30. Evolución de peatones por año

### DataFrame (Microsoft.Data.Analysis)

Implementar las **mismas 30 consultas** usando DataFrame.

## Justificación del Diseño

El alumno debe incluir un documento o sección en el README justificando **todas** sus decisiones de diseño.

> ⚠️ **Importante:** No basta con mostrar el código. Debes:
> - **Justificar cada decisión**: por qué elegiste un enfoque u otro
> - **Mostrar los tiempos de cada ejecución** (lectura de ficheros, LINQ, DataFrames)
> - **Analizar por qué obtienes esos resultados**: ¿por qué una consulta es más rápida que otra? ¿por qué en algunos casos una técnica empeora?

### Puntos de reflexión

- No hay una única respuesta correcta. Lo importante es que puedas **justificar** tu decisión.
- ¿Qué aprenderías para la próxima vez que proceses datos masivos?

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
│   ├── 2024_Accidentalidad.csv      ← Descargado de datos.madrid.es
│   ├── 2025_Accidentalidad.csv      ← Descargado de datos.madrid.es
│   └── 2026_Accidentalidad.csv      ← Descargado de datos.madrid.es
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
│   ├── AccidentesLinqAnalyzer.cs
│   └── AccidentesDataFrameAnalyzer.cs
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
└── README.md
```

### Ejemplo de uso combinado

El proyecto debe mostrar una comparativa de tiempos al final.

---

## Comparativa de Tiempos

El programa debe mostrar una comparativa detallada de tiempos de ejecución.

---

## Entrega

Sube el proyecto a tu repositorio GitHub con el nombre `AccidentesMadrid`.

Incluye:
1. Código fuente completo
2. Ficheros CSV en `data/` (2024, 2025, 2026)
3. Dockerfile y docker-compose.yml
4. README con:
   - Instrucciones de uso
   - **Justificación del diseño** (todas las decisiones tomadas y por qué)
   - **Tiempos de ejecución** (lectura de ficheros, LINQ, DataFrames)
   - **Análisis de resultados** (por qué obtienes esos tiempos)
