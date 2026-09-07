# SQL — Instrucciones de Supervivencia

> Referencia rápida de SQL para PostgreSQL, MySQL y SQLite.

## Consultas básicas

```sql
-- Seleccionar todo
SELECT * FROM productos;

-- Seleccionar columnas específicas
SELECT nombre, precio FROM productos;

-- Con condición
SELECT * FROM productos WHERE precio > 100;

-- Ordenar
SELECT * FROM productos ORDER BY precio DESC;
SELECT * FROM productos ORDER BY categoria, nombre;

-- Limitar resultados
SELECT * FROM productos LIMIT 10;          -- PostgreSQL/MySQL
SELECT TOP 10 * FROM productos;            -- SQL Server
```

> 💡 **Consejo:** Evita `SELECT *` en producción. Sé explícito con las columnas: es más rápido y claro.

> 🔧 **Truco:** En PostgreSQL, `LIMIT` va al final. En SQL Server, `TOP` va después de `SELECT`.

## Insertar datos

```sql
-- Un registro
INSERT INTO productos (nombre, precio, categoria)
VALUES ('Portátil ASUS', 899.99, 'Electrónica');

-- Múltiples registros
INSERT INTO productos (nombre, precio, categoria)
VALUES 
    ('Teclado', 49.99, 'Periféricos'),
    ('Ratón', 29.99, 'Periféricos');
```

### Obtener la PK tras INSERTar

Cada motor lo resuelve de forma distinta:

```sql
-- ✅ PostgreSQL: RETURNING (lo más limpio)
INSERT INTO productos (nombre, precio, categoria)
VALUES ('Portátil ASUS', 899.99, 'Electrónica')
RETURNING id;

-- ✅ MariaDB / MySQL: LAST_INSERT_ID()
INSERT INTO productos (nombre, precio, categoria)
VALUES ('Portátil ASUS', 899.99, 'Electrónica');
SELECT LAST_INSERT_ID();   -- devuelve la PK del último INSERT

-- ✅ SQLite: last_insert_rowid()
INSERT INTO productos (nombre, precio, categoria)
VALUES ('Portátil ASUS', 899.99, 'Electrónica');
SELECT last_insert_rowid();   -- función interna de SQLite
```

> 💡 **Consejo:** En Dapper/C#, para PostgreSQL usa `RETURNING id` directo en el SQL.
> Para MySQL/SQLite, ejecuta el INSERT y luego `SELECT LAST_INSERT_ID()` o `last_insert_rowid()` como segunda query.

## Actualizar datos

```sql
-- Con condición
UPDATE productos 
SET precio = 799.99 
WHERE id = 1;

-- Sin WHERE (¡CUIDADO! actualiza todo)
UPDATE productos SET activo = true;
```

> ⚠️ **Advertencia:** Un `UPDATE` sin `WHERE` actualiza TODAS las filas. Siempre incluye un `WHERE` a menos que realmente quieras cambiar todo.

## Eliminar datos

```sql
-- Con condición
DELETE FROM productos WHERE id = 1;

-- Sin WHERE (¡CUIDADO! borra todo)
DELETE FROM productos;

-- TRUNCATE (más rápido, reinicia identity)
TRUNCATE TABLE productos;
TRUNCATE TABLE productos RESTART IDENTITY;  -- PostgreSQL
```

> 💡 **Consejo:** `TRUNCATE` es más rápido que `DELETE` para vaciar una tabla porque no genera logs por fila. Pero no ejecuta triggers.

> 🔧 **Truco:** Si quieres borrar todo pero manteniendo la estructura, usa `TRUNCATE`. Si necesitas que se ejecuten triggers, usa `DELETE FROM` sin `WHERE`.

## Crear tablas

```sql
CREATE TABLE productos (
    id SERIAL PRIMARY KEY,           -- PostgreSQL
    -- id INTEGER PRIMARY KEY AUTOINCREMENT,  -- SQLite
    nombre VARCHAR(100) NOT NULL,
    precio DECIMAL(10, 2) NOT NULL DEFAULT 0,
    categoria VARCHAR(50),
    activo BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

> 💡 **Consejo:** `NOT NULL` evita valores vacíos. `DEFAULT` proporciona un valor si no se especifica. Usa ambos para evitar bugs.

> ⚠️ **Advertencia:** El `id SERIAL` es de PostgreSQL. En SQLite usa `INTEGER PRIMARY KEY AUTOINCREMENT`. En MySQL usa `INT AUTO_INCREMENT PRIMARY KEY`.

## Tipos de datos comunes

| Tipo | PostgreSQL | SQLite | MySQL |
|------|-----------|--------|-------|
| Entero | `INTEGER` | `INTEGER` | `INT` |
| Decimal | `DECIMAL(10,2)` | `REAL` | `DECIMAL(10,2)` |
| Texto | `VARCHAR(n)` | `TEXT` | `VARCHAR(n)` |
| Booleano | `BOOLEAN` | `INTEGER (0/1)` | `TINYINT(1)` |
| Fecha | `TIMESTAMP` | `TEXT` | `DATETIME` |
| Texto largo | `TEXT` | `TEXT` | `TEXT` |

## Consultas con JOIN

```sql
-- INNER JOIN (solo coincidencias)
SELECT p.nombre, c.nombre AS categoria
FROM productos p
INNER JOIN categorias c ON p.categoria_id = c.id;

-- LEFT JOIN (todos los de la izquierda)
SELECT p.nombre, c.nombre AS categoria
FROM productos p
LEFT JOIN categorias c ON p.categoria_id = c.id;

-- RIGHT JOIN (todos los de la derecha)
SELECT p.nombre, c.nombre AS categoria
FROM productos p
RIGHT JOIN categorias c ON p.categoria_id = c.id;
```

> 💡 **Analogía:** `INNER JOIN` es la intersección de dos conjuntos. `LEFT JOIN` es "todo lo de la izquierda, y si hay coincidencia lo de la derecha". En la práctica, el 90% de las veces usas `LEFT JOIN`.

> 🔧 **Truco:** Si no sabes qué JOIN usar, empieza con `LEFT JOIN`. Si sobran filas, cámbialo a `INNER JOIN`.

## Agrupaciones

```sql
-- Contar por categoría
SELECT categoria, COUNT(*) AS total
FROM productos
GROUP BY categoria;

-- Con filtro de agrupación
SELECT categoria, COUNT(*) AS total
FROM productos
GROUP BY categoria
HAVING COUNT(*) > 5;

-- Media, suma, mínimo, máximo
SELECT 
    categoria,
    COUNT(*) AS total,
    AVG(precio) AS media_precio,
    SUM(precio) AS suma_total,
    MIN(precio) AS min_precio,
    MAX(precio) AS max_precio
FROM productos
GROUP BY categoria;
```

> 💡 **Consejo:** `WHERE` filtra ANTES de agrupar. `HAVING` filtra DESPUÉS de agrupar. No confundas ambos.

## Subconsultas

```sql
-- Subconsulta simple
SELECT * FROM productos 
WHERE categoria_id = (SELECT id FROM categorias WHERE nombre = 'Electrónica');

-- IN con subconsulta
SELECT * FROM productos 
WHERE categoria_id IN (SELECT id FROM categorias WHERE activa = true);
```

## Índices

```sql
-- Crear índice (acelera búsquedas)
CREATE INDEX idx_productos_categoria ON productos(categoria_id);

-- Índice único
CREATE UNIQUE INDEX idx_productos_nombre ON productos(nombre);

-- Eliminar índice
DROP INDEX idx_productos_categoria;
```

> 💡 **Consejo:** Los índices aceleran las búsquedas (`WHERE`, `JOIN`, `ORDER BY`) pero ralentizan los `INSERT`/`UPDATE`. No pongas un índice en cada columna.

> 🔧 **Truco:** Crea un índice en las columnas que usas en `WHERE` y `JOIN` frecuentemente. Si una tabla tiene millones de filas, un índice puede pasar una consulta de 5s a 5ms.

## Transacciones

```sql
BEGIN;
    UPDATE cuentas SET saldo = saldo - 100 WHERE id = 1;
    UPDATE cuentas SET saldo = saldo + 100 WHERE id = 2;
COMMIT;  -- confirmar cambios

-- Si hay error
ROLLBACK;  -- deshacer todo
```

> ⚠️ **Advertencia:** Si haces `BEGIN` y el proceso muere sin `COMMIT` ni `ROLLBACK`, la transacción queda abierta y bloquea la tabla. En PostgreSQL puedes verlas con `SELECT * FROM pg_stat_activity WHERE state = 'idle in transaction'`.

## Funciones comunes

```sql
-- Concatenar
SELECT nombre || ' - ' || categoria FROM productos;

-- COALESCE (valor por defecto si es NULL)
SELECT COALESCE(descripcion, 'Sin descripción') FROM productos;

-- CASE (condicional)
SELECT 
    nombre,
    CASE 
        WHEN precio < 50 THEN 'Barato'
        WHEN precio < 200 THEN 'Medio'
        ELSE 'Caro'
    END AS rango_precio
FROM productos;

--日期
SELECT NOW();                          -- fecha y hora actual
SELECT CURRENT_DATE;                   -- solo fecha
SELECT EXTRACT(YEAR FROM created_at);  -- extraer año
```

## Errores comunes

| Error | Causa típica |
|-------|-------------|
| `column "X" does not exist` | Nombre de columna mal escrito |
| `syntax error at or near "X"` | Falta coma, paréntesis o comilla |
| `duplicate key value` | Intentar insertar ID que ya existe |
| `foreign key constraint` | Referenciar ID que no existe en otra tabla |
| `relation "X" does not exist` | Tabla no existe o nombre mal escrito |
