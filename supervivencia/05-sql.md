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

## Actualizar datos

```sql
-- Con condición
UPDATE productos 
SET precio = 799.99 
WHERE id = 1;

-- Sin WHERE (¡CUIDADO! actualiza todo)
UPDATE productos SET activo = true;
```

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

## Transacciones

```sql
BEGIN;
    UPDATE cuentas SET saldo = saldo - 100 WHERE id = 1;
    UPDATE cuentas SET saldo = saldo + 100 WHERE id = 2;
COMMIT;  -- confirmar cambios

-- Si hay error
ROLLBACK;  -- deshacer todo
```

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
