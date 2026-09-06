- [13. LINQ en Colecciones y Base de Datos](#13-linq-en-colecciones-y-base-de-datos)
  - [13.1. LINQ como Lenguaje Declarativo](#131-linq-como-lenguaje-declarativo)
  - [13.2. Operaciones Fundamentales](#132-operaciones-fundamentales)
  - [13.3. GroupBy: La Operación Más Poderosa](#133-groupby-la-operación-más-poderosa)
  - [13.4. JOINs en LINQ](#134-joins-en-linq)
  - [13.5. LINQ en Base de Datos (Entity Framework Core)](#135-linq-en-base-de-datos-entity-framework-core)
  - [13.6. Parallel LINQ (PLINQ)](#136-parallel-linq-plinq)
  - [13.7. LINQ Avanzado](#137-linq-avanzado)


# 13. LINQ en Colecciones y Base de Datos

> 💡 **Punto de partida:** Sin LINQ, para filtrar una lista de clientes tendrías que escribir un `foreach`, crear una lista nueva, añadir elementos que cumplan la condición... Con LINQ, lo haces en una línea. Pero LINQ no es solo comodidad: es un lenguaje declarativo que transforma la forma en que piensas sobre datos.

En este tema aprenderás a usar LINQ en colecciones y bases de datos, las operaciones más importantes, cómo hacer JOINs, cuándo usar PLINQ y por qué `GroupBy + ToDictionary` es más eficiente que `GroupBy + Select + ToList`.

**Objetivos de aprendizaje:**

- Entender la diferencia entre programación imperativa y declarativa
- Dominar las operaciones fundamentales: Where, Select, OrderBy, GroupBy
- Conocer la eficiencia de GroupBy + ToDictionary vs GroupBy + Select + ToList
- Hacer JOINs entre colecciones
- Usar PLINQ para procesamiento paralelo

## 13.1. LINQ como Lenguaje Declarativo

**LINQ** (Language Integrated Query) permite escribir consultas sobre datos de forma declarativa: describes **qué** quieres, no **cómo** obtenerlo.

```csharp
// IMPERATIVO: CÓMO hacerlo paso a paso
var resultados = new List<Cliente>();
foreach (var cliente in clientes)
{
    if (cliente.Ciudad == "Madrid" && cliente.Activo)
    {
        resultados.Add(cliente);
    }
}
resultados.Sort((a, b) => a.Nombre.CompareTo(b.Nombre));

// DECLARATIVO: QUÉ quieres
var resultados = clientes
    .Where(c => c.Ciudad == "Madrid" && c.Activo)
    .OrderBy(c => c.Nombre)
    .ToList();
```

| Imperativo | Declarativo (LINQ) |
|------------|-------------------|
| Describe **cómo** hacerlo | Describe **qué** quieres |
| Bucles `for/while` | Pipeline de operaciones |
| Modifica estado | Inmutable (nuevas colecciones) |
| Más verboso | Más conciso y legible |

> 💡 **Analogía:** Imperativo es como dar instrucciones paso a paso a alguien que va al supermercado: "Entra, gira a la derecha, coge una cesta, ve al pasillo 3, coge leche...". Declarativo es como decir: "Trae leche, pan y huevos". No te importa el camino, solo el resultado.

## 13.2. Operaciones Fundamentales

### Filtrado: Where

```csharp
var caros = productos.Where(p => p.Precio > 100);
var madrid = clientes.Where(c => c.Ciudad == "Madrid");
```

### Proyección: Select

```csharp
var nombres = productos.Select(p => p.Nombre);
var resumen = productos.Select(p => new { p.Nombre, p.Precio });
```

### Ordenación: OrderBy / ThenBy

```csharp
var ordenados = clientes
    .OrderBy(c => c.Apellidos)      // Primero por apellidos
    .ThenBy(c => c.Nombre);         // Luego por nombre
```

### Agregación: Count, Sum, Average, Max, Min

```csharp
int total = productos.Count();
decimal suma = productos.Sum(p => p.Precio);
double media = productos.Average(p => p.Precio);
decimal maximo = productos.Max(p => p.Precio);
```

### Partitionado: Take, Skip

```csharp
var primeraPagina = productos.Take(10);     // Primeros 10
var segundaPagina = productos.Skip(10).Take(10); // Siguientes 10
```

### Búsqueda: First, Single, Any, All

```csharp
var primero = productos.First(p => p.Precio > 100);      // Primer elemento o excepción
var alguno = productos.Any(p => p.Precio > 100);          // true/false
var todos = productos.All(p => p.Precio > 0);             // true/false
```

> 📝 **Nota:** `First()` lanza excepción si no hay elementos. `FirstOrDefault()` devuelve `null`. Usa `FirstOrDefault()` cuando el elemento puede no existir.

## 13.3. GroupBy: La Operación Más Poderosa

`GroupBy` agrupa elementos por una clave. Es muy potente pero hay que saber usarlo bien.

### GroupBy + Select + ToList (menos eficiente)

```csharp
// Agrupar por categoría y obtener lista de productos
var porCategoria = productos
    .GroupBy(p => p.Categoria)                    // Agrupa
    .Select(g => new                              // Transforma
    {
        Categoria = g.Key,
        Productos = g.ToList(),                   // Materializa cada grupo
        Total = g.Count()
    })
    .ToList();                                    // Materializa todo
```

### GroupBy + ToDictionary (más eficiente)

```csharp
// Lo mismo pero más eficiente: acceso directo por clave
var porCategoria = productos
    .GroupBy(p => p.Categoria)
    .ToDictionary(
        g => g.Key,                               // Clave del diccionario
        g => g.ToList()                           // Valor del diccionario
    );

// Ahora puedes acceder directamente:
var electronicos = porCategoria["Electrónica"];   // O(1) en vez de O(n)
```

### Comparativa de rendimiento

| Operación | Memoria | Tiempo acceso | Cuándo usar |
|-----------|---------|---------------|-------------|
| `GroupBy + Select + ToList` | Lista de listas | O(n) buscar | Cuando necesitas iterar todos |
| `GroupBy + ToDictionary` | Diccionario | O(1) buscar | Cuando buscas por clave |
| `GroupBy + ToLookup` | Lookup (hash) | O(1) buscar | Claves duplicadas permitidas |

```csharp
// ToLookup: como ToDictionary pero permite claves duplicadas
var lookup = productos.ToLookup(p => p.Categoria);
var electronicos = lookup["Electrónica"]; // Puede tener múltiples elementos
```

> 💡 **Consejo:** Si vas a buscar por clave después de agrupar, usa `ToDictionary` o `ToLookup`. Si solo vas a iterar todos los grupos, `GroupBy + Select + ToList` está bien.

📌 **Ejemplo real:** En una app de e-commerce, para mostrar el catálogo agrupado por categoría en la web, `ToDictionary` es más eficiente porque el usuario puede hacer clic en una categoría específica y necesitas acceso rápido.

## 13.4. JOINs en LINQ

Los JOINs combinan datos de dos o más colecciones basándose en una relación:

```csharp
// Datos
var clientes = new[]
{
    new { Id = 1, Nombre = "Ana", CiudadId = 1 },
    new { Id = 2, Nombre = "Carlos", CiudadId = 2 },
    new { Id = 3, Nombre = "María", CiudadId = 1 }
};

var ciudades = new[]
{
    new { Id = 1, Nombre = "Madrid" },
    new { Id = 2, Nombre = "Barcelona" }
};

// Inner Join: solo coincidencias
var resultado = clientes
    .Join(ciudades,
        cliente => cliente.CiudadId,     // Clave del primer conjunto
        ciudad => ciudad.Id,             // Clave del segundo conjunto
        (cliente, ciudad) => new         // Resultado
        {
            cliente.Nombre,
            Ciudad = ciudad.Nombre
        });
// Ana → Madrid, Carlos → Barcelona, María → Madrid
```

### Tipos de JOIN

| Tipo | Descripción | SQL equivalente |
|------|-------------|-----------------|
| **Inner Join** | Solo coincidencias en ambas colecciones | `INNER JOIN` |
| **Left Join** | Todos los de la izquierda, con coincidencias o null | `LEFT JOIN` |
| **Group Join** | Agrupa los elementos de la derecha por la izquierda | `GROUP BY` |

```csharp
// Left Join: todos los clientes, tengan o no ciudad
var resultado = clientes
    .GroupJoin(ciudades,
        cliente => cliente.CiudadId,
        ciudad => ciudad.Id,
        (cliente, ciudadesGrupo) => new { cliente, ciudadesGrupo })
    .SelectMany(
        x => x.ciudadesGrupo.DefaultIfEmpty(),
        (x, ciudad) => new
        {
            x.cliente.Nombre,
            Ciudad = ciudad?.Nombre ?? "Sin ciudad"
        });
```

> 📝 **Nota:** En Entity Framework Core, los JOINs se escriben con `Join()` o `GroupJoin()`, pero también puedes usar `Include()` para cargar relaciones de forma más sencilla.

## 13.5. LINQ en Base de Datos (Entity Framework Core)

Cuando usas LINQ con Entity Framework Core, las consultas se traducen a SQL automáticamente:

```csharp
// LINQ se traduce a SQL
var resultado = context.Productos
    .Where(p => p.Precio > 100)           // WHERE Precio > 100
    .OrderBy(p => p.Nombre)               // ORDER BY Nombre
    .Select(p => new                       // SELECT Nombre, Precio
    {
        p.Nombre,
        p.Precio
    })
    .ToList();                             // Ejecuta la consulta SQL
```

> ⚠️ **Advertencia:** No todas las operaciones LINQ se traducen a SQL. Por ejemplo, métodos C# como `.ToString()` o `.Contains()` con expresiones complejas pueden dar error en runtime. Usa `EF.Functions` para funciones SQL específicas.

## 13.6. Parallel LINQ (PLINQ)

**PLINQ** ejecuta consultas LINQ en paralelo usando múltiples núcleos de CPU:

```csharp
// LINQ secuencial (un núcleo)
var resultado = productos
    .Where(p => p.Precio > 100)
    .Select(p => ProcesarProducto(p))  // Procesamiento pesado
    .ToList();

// PLINQ (múltiples núcleos)
var resultado = productos
    .AsParallel()
    .Where(p => p.Precio > 100)
    .Select(p => ProcesarProducto(p))  // Se ejecuta en paralelo
    .ToList();
```

| LINQ | PLINQ |
|------|-------|
| Un núcleo de CPU | Múltiples núcleos |
| Orden garantizado | Orden NO garantizado (usa `.AsOrdered()` si lo necesitas) |
| Sin overhead | Overhead de sincronización |
| Para colecciones pequeñas | Para colecciones grandes + procesamiento pesado |

> 💡 **Consejo:** Usa PLINQ solo cuando el procesamiento de cada elemento es **pesado** (cálculos complejos, llamadas a servicios externos). Para operaciones simples, el overhead de PLINQ puede hacer que sea más lento que LINQ secuencial.

## 13.7. LINQ Avanzado

### SelectMany: Aplanar colecciones anidadas

```csharp
var pedidos = new[]
{
    new { Cliente = "Ana", Productos = new[] { "Laptop", "Ratón" } },
    new { Cliente = "Carlos", Productos = new[] { "Teclado" } }
};

// Sin SelectMany: colección de colecciones
var productosAnidados = pedidos.Select(p => p.Productos);

// Con SelectMany: lista plana
var todosLosProductos = pedidos.SelectMany(p => p.Productos);
// ["Laptop", "Ratón", "Teclado"]
```

### Zip: Combinar secuencias

```csharp
var nombres = new[] { "Ana", "Carlos", "María" };
var edades = new[] { 25, 30, 28 };

var combinado = nombres.Zip(edades, (nombre, edad) => new { nombre, edad });
// [{Ana, 25}, {Carlos, 30}, {María, 28}]
```

### Distinct / Except / Intersect: Operaciones de conjuntos

```csharp
var lista1 = new[] { 1, 2, 3, 4, 5 };
var lista2 = new[] { 4, 5, 6, 7, 8 };

var sinDuplicados = lista1.Distinct();           // [1, 2, 3, 4, 5]
var soloEn1 = lista1.Except(lista2);             // [1, 2, 3]
var comunes = lista1.Intersect(lista2);          // [4, 5]
var todos = lista1.Union(lista2);                // [1, 2, 3, 4, 5, 6, 7, 8]
```

### Chunk: Dividir en trozos

```csharp
var numeros = Enumerable.Range(1, 100);
var porciones = numeros.Chunk(10); // 10 grupos de 10 elementos

foreach (var porcion in porciones)
{
    Console.WriteLine($"Grupo: {string.Join(", ", porcion)}");
}
```

---

**Resumen del punto:**

| Concepto | Descripción |
|----------|-------------|
| **LINQ** | Lenguaje declarativo para consultas sobre datos |
| **Where** | Filtra elementos que cumplen una condición |
| **Select** | Transforma cada elemento |
| **GroupBy** | Agrupa por una clave |
| **GroupBy + ToDictionary** | Más eficiente que GroupBy + Select + ToList |
| **Join** | Combina datos de dos colecciones |
| **PLINQ** | LINQ en paralelo con múltiples núcleos |
| **IQueryable** | Consultas que se traducen a SQL |

En el siguiente punto veremos cómo trabajar con ficheros y formatos de intercambio: IDisposable, System.IO, CSV con CsvHelper y JSON con System.Text.Json.
