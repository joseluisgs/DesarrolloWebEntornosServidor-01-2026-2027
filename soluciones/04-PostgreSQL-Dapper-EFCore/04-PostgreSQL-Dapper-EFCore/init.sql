-- Ejemplo 03: PostgreSQL con Dapper y EF Core
-- Script de inicialización de la base de datos

CREATE TABLE IF NOT EXISTS productos (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    precio NUMERIC(10, 2) NOT NULL,
    categoria VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Datos de ejemplo
INSERT INTO productos (nombre, precio, categoria) VALUES
    ('Portátil ASUS', 899.99, 'Electrónica'),
    ('Teclado Mecánico', 79.50, 'Periféricos'),
    ('Monitor 27"', 349.00, 'Electrónica'),
    ('Ratón Gaming', 45.99, 'Periféricos'),
    ('Disco SSD 1TB', 89.90, 'Almacenamiento');
