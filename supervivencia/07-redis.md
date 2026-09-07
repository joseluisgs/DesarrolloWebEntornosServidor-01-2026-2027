# Redis — Instrucciones de Supervivencia

> Referencia rápida de Redis (cli y comandos habituales).

## Conexión

```bash
# Conectar a Redis local
redis-cli

# Conectar con credenciales
redis-cli -u redis://:password@localhost:6379

# Conectar a host/puerto específicos
redis-cli -h localhost -p 6379

# Probar conexión
redis-cli ping    # debe responder PONG
```

## Strings

```bash
# Guardar valor
SET nombre "José"
SET contador 10
SET sesion:abc123 "datos" EX 3600   # con expiración en segundos

# Obtener valor
GET nombre         # "José"
GET contador       # "10"
GET inexistente    # (nil)

# Operaciones atómicas
INCR contador      # 11
INCRBY contador 5  # 16
DECR contador      # 15

# Operaciones con strings
APPEND nombre " García"    # "José García"
STRLEN nombre              # 12
```

## Hashes (objetos)

```bash
# Guardar campo
HSET usuario:1 nombre "Ana" email "ana@email.com" edad 25

# Obtener campo
HGET usuario:1 nombre      # "Ana"
HGETALL usuario:1          # todos los campos

# Obtener múltiples campos
HMGET usuario:1 nombre email

# Eliminar campo
HDEL usuario:1 edad

# Ver si existe
HEXISTS usuario:1 nombre   # 1 (true)

# Contar campos
HLEN usuario:1             # 2
```

## Listas

```bash
# Añadir elementos
RPUSH cola "tarea1" "tarea2" "tarea3"   # al final
LPUSH cola "tarea0"                      # al principio

# Obtener elementos
LRANGE cola 0 -1       # todos
LRANGE cola 0 0        # primero
LINDEX cola -1          # último

# Sacar elementos
RPOP cola               # sacar del final
LPOP cola               # sacar del principio
BLPOP cola 30           # bloquear hasta que haya dato (30s timeout)

# Longitud
LLEN cola
```

## Sets (conjuntos)

```bash
# Añadir elementos
SADD tags "csharp" "dotnet" "backend"

# Obtener miembros
SMEMBERS tags

# Operaciones
SISMEMBER tags "csharp"    # 1 (true)
SCARD tags                  # 3 (conteo)
SREM tags "backend"         # eliminar

# Intersección, unión, diferencia
SADD tags2 "csharp" "frontend"
SINTER tags tags2           # {"csharp"} (comunes)
SUNION tags tags2           # unión
SDIFF tags tags2            # solo en tags
```

## Sorted Sets

```bash
# Añadir con puntuación
ZADD leaderboard 100 "jugador1" 200 "jugador2" 150 "jugador3"

# Obtener por rango
ZRANGE leaderboard 0 -1 WITHSCORES    # todos, de menor a mayor
ZREVRANGE leaderboard 0 2 WITHSCORES  # top 3

# Obtener puntuación
ZSCORE leaderboard "jugador1"          # 100

# Contar
ZCARD leaderboard

# Eliminar
ZREM leaderboard "jugador1"
```

## Expiración y TTL

```bash
# Establecer expiración (segundos)
EXPIRE clave 3600            # 1 hora
SETEX clave 3600 "valor"     # set + expire en uno

# Establecer expiración (milisegundos)
PEXPIRE clave 3600000

# Ver tiempo restante
TTL clave                    # -2 (no existe), -1 (sin expirar), o segundos
PTTL clave                   # en milisegundos

# Quitar expiración
PERSIST clave
```

## Gestión de claves

```bash
# Ver claves
KEYS *                        # todas (¡CUIDADO en producción!)
KEYS usuario:*                # por patrón
SCAN 0 MATCH "usuario:*" COUNT 100  # seguro para producción

# Verificar si existe
EXISTS clave                  # 1 (true), 0 (false)

# Tipo de dato
TYPE clave

# Renombrar
RENAME clave nueva_clave

# Eliminar
DEL clave                     # una
UNLINK clave                  # una (asíncrono, mejor en producción)

# Contar claves
DBSIZE
```

## Transacciones

```bash
# Iniciar transacción
MULTI
SET cuenta1 900
SET cuenta2 1100
EXEC   # ejecutar todo

# Descartar
DISCARD
```

## Scripting (Lua)

```bash
# Ejecutar script Lua
EVAL "return redis.call('SET', KEYS[1], ARGV[1])" 1 mi_clave mi_valor
```

## Monitoreo y diagnóstico

```bash
# Ver info del servidor
INFO

# Ver info de memoria
INFO memory

# Ver info de clientes
INFO clients

# Monitorizar comandos en tiempo real (¡SOLO en desarrollo!)
MONITOR

# Ver uso de memoria de una clave
MEMORY USAGE clave
```

## Errores comunes

| Error | Causa típica |
|-------|-------------|
| `NOAUTH Authentication required` | Falta autenticar: `AUTH password` |
| `WRONGTYPE Operation against key` | Operar con tipo incorrecto (ej: GET en una lista) |
| `OOM command not allowed` | Sin memoria, revisar `maxmemory-policy` |
| `ERR wrong number of arguments` | Falta un argumento en el comando |
| `NOSCRIPT No matching script` | Script Lua no registrado |
