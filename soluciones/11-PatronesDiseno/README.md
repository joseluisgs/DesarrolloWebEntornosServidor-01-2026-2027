# Ejemplo 11: Patrones de Diseño y SOLID

## Descripción

Ejemplo completo que demuestra los 5 principios SOLID y 6 patrones de diseño aplicados a escenarios reales de desarrollo web.

## Objetivos de aprendizaje

- Entender los 5 principios SOLID con ejemplos negativos y positivos
- Aplicar Strategy para intercambiar algoritmos en runtime
- Usar Factory para crear objetos sin acoplamiento
- Añadir comportamiento con Decorator sin modificar código
- Integrar APIs externas con Adapter
- Construir objetos complejos con Builder

## SOLID

| Principio | Ejemplo | Demo |
|-----------|---------|------|
| **S** (SRP) | Una clase = una responsabilidad | `PedidoRepository`, `EmailService`, `FacturaService` separados |
| **O** (OCP) | Abierto a extensión, cerrado a modificación | `INotificador` con Email/SMS/Push — añadir tipos sin modificar |
| **L** (LSP) | Subtipos sustituibles | `Rectangulo` y `Cuadrado` implementan `IFigura` correctamente |
| **I** (ISP) | Interfaces pequeñas | `IReadRepository` vs `IWriteRepository` — solo lo que necesitas |
| **D** (DIP) | Depender de abstracciones | `PedidoServiceDip` depende de `IPedidoRepository`, no de `PedidoRepository` |

## Patrones de Diseño

| Patrón | Qué hace | Ejemplo real |
|--------|----------|--------------|
| **Strategy** | Intercambiar algoritmos | Validación de Persona, ordenación por nombre/edad |
| **Factory** | Crear objetos sin `new` | `PersonaRepositoryFactory.Crear("Memory")` |
| **Decorator** | Añadir comportamiento | Logging + Métricas + Retry encadenados |
| **Adapter** | Convertir interfaz | API externa → `IUsuarioService` |
| **Builder** | Construir paso a paso | `ServidorConfigBuilder().ConHttps().ConCors().Build()` |

## Ejecución

```bash
dotnet run
```

## Resultado Esperado

```
=== Ejemplo 11: Patrones de Diseño y SOLID ===

--- SRP: Single Responsibility Principle ---
[LOG] Guardado: Café
Email a ana@email.com: Tu pedido: Café
Factura: Factura: Café - 2.50€

--- OCP: Open/Closed Principle ---
📧 Email: Nuevo pedido recibido
📱 SMS: Nuevo pedido recibido
🔔 Push: Nuevo pedido recibido

--- ISP: Interface Segregation Principle ---
IReadRepository: solo lectura (Find, GetById, GetAll)
IWriteRepository: solo escritura (Create, Update, Delete)

--- DIP: Dependency Inversion Principle ---
Guardado: Te
Email a bob@email.com: Pedido Te creado

--- STRATEGY: Validación y Ordenación ---
✅ Persona válida: Ana
❌ Errores: Nombre requerido, Email inválido, Edad fuera de rango (0-150)
Orden por nombre: Ana, Bob, Carlos
Orden por edad: Ana, Carlos, Bob

--- FACTORY: Repositorio según configuración ---
Memory: Ana
Json: 2 usuarios

--- DECORATOR: Logging + Métricas + Retry ---
[LOG] Antes de Guardar
[METRICS] Guardar llamado 1 veces
[LOG] Después de Guardar
[LOG] Antes de Obtener(1)
[METRICS] Obtener llamado 1 veces
[LOG] Después de Obtener(1): encontrado
Resultado: Ana

--- ADAPTER: API externa → nuestra interfaz ---
Usuario: Ana García (ana@email.com)
Usuario 999 no encontrado

--- BUILDER: Configuración de servidor ---
Configuración: Host=miapi.com:443 HTTPS=True Timeout=60s MaxConexiones=500 CORS=True Origenes=[https://miapp.com, https://www.miapp.com]
```
