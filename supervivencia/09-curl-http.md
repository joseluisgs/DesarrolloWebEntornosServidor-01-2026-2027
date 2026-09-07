# curl & HTTP — Comandos de Supervivencia

> Para probar APIs REST, endpoints y servicios web desde la terminal.

## GET — Obtener datos

```bash
# Básico
curl https://jsonplaceholder.typicode.com/users

# Con verbosidad (ver cabeceras)
curl -v https://jsonplaceholder.typicode.com/users

# Solo cabeceras
curl -I https://jsonplaceholder.typicode.com/users

# Formateado con jq (si está instalado)
curl -s https://jsonplaceholder.typicode.com/users | jq

# Con autenticación
curl -H "Authorization: Bearer token123" https://api.ejemplo.com/datos

# Con parámetros de query
curl "https://api.ejemplo.com/buscar?q=dotnet&page=1"

# Guardar en fichero
curl -o respuesta.json https://jsonplaceholder.typicode.com/users

# Seguir redirecciones
curl -L https://ejemplo.com
```

## POST — Crear datos

```bash
# JSON
curl -X POST https://jsonplaceholder.typicode.com/users \
  -H "Content-Type: application/json" \
  -d '{"name": "José", "email": "jose@email.com"}'

# Desde fichero
curl -X POST https://api.ejemplo.com/users \
  -H "Content-Type: application/json" \
  -d @usuario.json

# Formulario
curl -X POST https://api.ejemplo.com/login \
  -d "username=admin&password=1234"

# Formulario multipart (archivos)
curl -X POST https://api.ejemplo.com/upload \
  -F "file=@foto.jpg" \
  -F "descripcion=Mi foto"
```

## PUT — Actualizar datos

```bash
# Actualizar recurso completo
curl -X PUT https://jsonplaceholder.typicode.com/users/1 \
  -H "Content-Type: application/json" \
  -d '{"name": "José García", "email": "jose@email.com"}'

# Actualizar desde fichero
curl -X PUT https://api.ejemplo.com/users/1 \
  -H "Content-Type: application/json" \
  -d @usuario_actualizado.json
```

## PATCH — Actualización parcial

```bash
# Solo cambiar campos específicos
curl -X PATCH https://jsonplaceholder.typicode.com/users/1 \
  -H "Content-Type: application/json" \
  -d '{"email": "nuevo@email.com"}'
```

## DELETE — Eliminar datos

```bash
# Eliminar recurso
curl -X DELETE https://jsonplaceholder.typicode.com/users/1

# Con autenticación
curl -X DELETE https://api.ejemplo.com/users/1 \
  -H "Authorization: Bearer token123"
```

## Cabeceras comunes

```bash
# Content-Type
-H "Content-Type: application/json"
-H "Content-Type: application/xml"
-H "Content-Type: multipart/form-data"

# Autenticación
-H "Authorization: Bearer <token>"
-H "Authorization: Basic <base64>"
-H "X-API-Key: mi-api-key"

# Aceptar respuesta
-H "Accept: application/json"
-H "Accept: text/html"

# Custom headers
-H "X-Request-Id: 12345"
-H "X-Correlation-Id: abc-def"
```

## Verbose y debugging

```bash
# Ver todo (cabeceras, request, response)
curl -v https://api.ejemplo.com/users

# Solo cabeceras de respuesta
curl -I https://api.ejemplo.com/users

# Ver tiempo de conexión
curl -w "Tiempo: %{time_total}s\n" -o /dev/null -s https://api.ejemplo.com/users

# Ver tiempos detallados
curl -o /dev/null -s -w "
DNS:       %{time_namelookup}s
Conexión:  %{time_connect}s
TLS:       %{time_appconnect}s
Total:     %{time_total}s
" https://api.ejemplo.com/users
```

## Proxies y timeouts

```bash
# Con proxy
curl -x http://proxy:8080 https://api.ejemplo.com/users

# Timeout (segundos)
curl --connect-timeout 5 https://api.ejemplo.com/users
curl --max-time 30 https://api.ejemplo.com/users

# Reintentar
curl --retry 3 --retry-delay 2 https://api.ejemplo.com/users
```

## httpie (alternativa moderna a curl)

```bash
# Instalar
pip install httpie

# GET
http GET https://jsonplaceholder.typicode.com/users

# POST con JSON
http POST https://api.ejemplo.com/users \
  name="José" email="jose@email.com"

# Con autenticación
http --auth=jose:password GET https://api.ejemplo.com/me

# Ver cabeceras
http --print=hHbB https://api.ejemplo.com/users
```

## Probar APIs locales (ASP.NET Core)

```bash
# Test rápido
curl https://localhost:5001/api/productos

# Ignorar certificado auto-firmado
curl -k https://localhost:5001/api/productos

# Con logging detallado
curl -v http://localhost:5000/api/productos 2>&1 | head -50
```

## Errores comunes

| Error | Causa típica | Solución |
|-------|-------------|----------|
| `Connection refused` | Servidor no está arrancado | `dotnet run` o verificar puerto |
| `SSL certificate problem` | Certificado auto-firmado | Usar `-k` o `--insecure` |
| `Could not resolve host` | URL mal escrita o sin internet | Verificar URL |
| `401 Unauthorized` | Falta autenticación | Añadir cabecera `Authorization` |
| `403 Forbidden` | Sin permisos | Revisar token/credenciales |
| `404 Not Found` | Endpoint no existe | Verificar URL del endpoint |
| `415 Unsupported Media Type` | Content-Type incorrecto | Añadir `-H "Content-Type: application/json"` |
| `500 Internal Server Error` | Error del servidor | Revisar logs del servidor |
