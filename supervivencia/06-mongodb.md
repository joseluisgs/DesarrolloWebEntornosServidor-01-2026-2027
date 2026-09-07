# MongoDB — Instrucciones de Supervivencia

> Referencia rápida de MongoDB (mongo shell y MongoDB Compass).

## Conexión

```bash
# Conectar a BD local
mongosh

# Conectar con credenciales
mongosh "mongodb://admin:password@localhost:27017"

# Conectar a BD específica
mongosh "mongodb://localhost:27017/mi_bd"
```

> 💡 **Consejo:** `mongosh` es el shell moderno (reemplaza a `mongo`). Si usas Docker, ejecuta: `docker exec -it mi-mongo mongosh`.

## Bases de datos y colecciones

```bash
# Ver bases de datos
show dbs

# Usar/base de datos
use mi_bd

# Ver colecciones
show collections

# Crear colección (automática al insertar)
db.createCollection("productos")

# Eliminar colección
db.productos.drop()

# Eliminar base de datos
db.dropDatabase()
```

> 🔧 **Truco:** En MongoDB, la BD se crea automáticamente al insertar el primer documento. No necesitas `CREATE DATABASE` como en SQL.

## Insertar documentos

```bash
# Un documento
db.productos.insertOne({
    nombre: "Portátil ASUS",
    precio: 899.99,
    categoria: "Electrónica",
    activo: true
})

# Múltiples documentos
db.productos.insertMany([
    { nombre: "Teclado", precio: 49.99, categoria: "Periféricos" },
    { nombre: "Ratón", precio: 29.99, categoria: "Periféricos" }
])
```

> 💡 **Consejo:** MongoDB genera un `_id` automático (ObjectId) si no lo especificas. No necesitas preocuparte por auto-increment como en SQL.

## Consultar documentos

```bash
# Todos
db.productos.find()

# Formateado
db.productos.find().pretty()

# Con filtro
db.productos.find({ precio: { $gt: 100 } })

# Un documento
db.productos.findOne({ nombre: "Portátil ASUS" })

# Contar
db.productos.countDocuments()
db.productos.countDocuments({ categoria: "Electrónica" })

# Limitar y ordenar
db.productos.find().sort({ precio: -1 }).limit(5)

# Seleccionar solo ciertos campos
db.productos.find({}, { nombre: 1, precio: 1, _id: 0 })
```

> 🔧 **Truco:** El segundo parámetro de `find()` es la proyección: `1` para mostrar, `0` para ocultar. Siempre oculta `_id: 0` si no lo necesitas.

## Operadores de consulta

| Operador | Significado | Ejemplo |
|----------|-------------|---------|
| `$eq` | Igual | `{ precio: { $eq: 100 } }` |
| `$ne` | No igual | `{ precio: { $ne: 100 } }` |
| `$gt` | Mayor que | `{ precio: { $gt: 100 } }` |
| `$gte` | Mayor o igual | `{ precio: { $gte: 100 } }` |
| `$lt` | Menor que | `{ precio: { $lt: 100 } }` |
| `$lte` | Menor o igual | `{ precio: { $lte: 100 } }` |
| `$in` | En lista | `{ categoria: { $in: ["A", "B"] } }` |
| `$nin` | No en lista | `{ categoria: { $nin: ["A"] } }` |
| `$and` | Y lógico | `{ $and: [{ precio: { $gt: 50 } }, { activo: true }] }` |
| `$or` | O lógico | `{ $or: [{ precio: { $lt: 10 } }, { precio: { $gt: 100 } }] }` |
| `$exists` | Campo existe | `{ descripcion: { $exists: true } }` |
| `$regex` | Expresión regular | `{ nombre: { $regex: "ASUS", $options: "i" } }` |

## Actualizar documentos

```bash
# Actualizar uno
db.productos.updateOne(
    { nombre: "Portátil ASUS" },
    { $set: { precio: 799.99 } }
)

# Actualizar muchos
db.productos.updateMany(
    { categoria: "Periféricos" },
    { $set: { activo: true } }
)

# Insertar si no existe
db.productos.updateOne(
    { nombre: "Nuevo" },
    { $setOnInsert: { precio: 9.99 } },
    { upsert: true }
)
```

> 💡 **Consejo:** `upsert: true` crea el documento si no existe. Es como un "insert or update". Muy útil para sincronizar datos.

## Eliminar documentos

```bash
# Eliminar uno
db.productos.deleteOne({ nombre: "Portátil ASUS" })

# Eliminar muchos
db.productos.deleteMany({ categoria: "Obsoleto" })

# Eliminar todos
db.productos.deleteMany({})
```

> ⚠️ **Advertencia:** `deleteMany({})` borra TODOS los documentos de la colección. Asegúrate de estar en la BD correcta con `db` antes de ejecutar.

## Índices

```bash
# Crear índice
db.productos.createIndex({ nombre: 1 })

# Índice único
db.productos.createIndex({ email: 1 }, { unique: true })

# Índice compuesto
db.productos.createIndex({ categoria: 1, precio: -1 })

# Ver índices
db.productos.getIndexes()

# Eliminar índice
db.productos.dropIndex("nombre_1")
```

> 💡 **Consejo:** Crea un índice en `_id` (ya existe por defecto) y en campos que usas en `find()` frecuentemente. El índice compuesto sirve para búsquedas que usan ambos campos.

## Aggregation Pipeline

```bash
# Agrupar y contar
db.productos.aggregate([
    { $group: { _id: "$categoria", total: { $sum: 1 } } }
])

# Ordenar resultado
db.productos.aggregate([
    { $group: { _id: "$categoria", total: { $sum: 1 } } },
    { $sort: { total: -1 } }
])

# Media de precios por categoría
db.productos.aggregate([
    { $group: { 
        _id: "$categoria", 
        media: { $avg: "$precio" },
        min: { $min: "$precio" },
        max: { $max: "$precio" }
    }}
])
```

> 💡 **Analogía:** Aggregation Pipeline es como una tubería de fábrica: cada etapa (`$group`, `$sort`, `$match`) recibe datos, los procesa y los pasa a la siguiente. Es el equivalente a las consultas SQL complejas pero por pasos.

> 🔧 **Truco:** Si no sabes qué pipeline usar, piensa en pasos: primero filtra (`$match`), luego agrupa (`$group`), luego ordena (`$sort`). Es más legible que una query gigante.

## ERRORES comunes

| Error | Causa típica |
|-------|-------------|
| `MongoServerError: E11000 duplicate key` | Intentar insertar campo con índice único duplicado |
| `MongoServerError: ns not found` | Colección o BD no existe |
| `MongoNetworkError: connect ECONNREFUSED` | MongoDB no está arrancado |
| `SyntaxError: Unexpected token` | Falta comilla o paréntesis en la consulta |
