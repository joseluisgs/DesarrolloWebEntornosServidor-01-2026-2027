- [22. Testing Avanzado en .NET](#22-testing-avanzado-en-net)
  - [22.1. NUnit: Framework de Tests](#221-nunit-framework-de-tests)
  - [22.2. FluentAssertions: Aserciones Expresivas](#222-fluentassertions-aserciones-expresivas)
  - [22.3. Moq: Mocking de Interfaces](#223-moq-mocking-de-interfaces)
  - [22.4. Patrón AAA: Arrange-Act-Assert](#224-patrón-aaa-arrange-act-assert)
  - [22.5. TestContainers: Tests con Docker](#225-testcontainers-tests-con-docker)
    - [22.5.1. Coverlet: Cobertura de Código](#2251-coverlet-cobertura-de-código)
  - [22.6. Depuración de Errores en C#](#226-depuración-de-errores-en-c)
  - [22.7. Regla: Nada se Entrega sin Tests](#227-regla-nada-se-entrega-sin-tests)


# 22. Testing Avanzado en .NET

> 💡 **Punto de partida:** Has escrito código, funciona... pero ¿y si mañana lo cambias y rompes algo? Los **tests automatizados** son tu red de seguridad: ejecutan tu código automáticamente y te avisan si algo se rompe. Pero no todos los tests son iguales. Hay tests unitarios (rápidos, aíslan componentes), tests de integración (prueban componentes juntos) y tests con contenedores (prueban contra bases de datos reales). Vamos a ver cómo escribir tests profesionales.

En este tema aprenderás NUnit, FluentAssertions, Moq, el patrón AAA y TestContainers para tests robustos y mantenibles.

**Objetivos de aprendizaje:**

- Escribir tests con NUnit: `[TestFixture]`, `[Test]`, `[SetUp]`, `[TestCase]`
- Usar FluentAssertions para aserciones legibles
- Mockear interfaces con Moq: `Setup`, `Verify`
- Aplicar el patrón AAA (Arrange-Act-Assert)
- Configurar TestContainers para tests con bases de datos reales

## 22.1. NUnit: Framework de Tests

### Instalación

```bash
dotnet add package NUnit
dotnet add package NUnit3TestAdapter
dotnet add package Microsoft.NET.Test.Sdk
```

**NUnit** es el framework de tests más usado en .NET. Los tests son clases con métodos decorados con atributos.

### Estructura de un test

```csharp
using NUnit.Framework;

[TestFixture] // Marca la clase como clase de test
public class CalculadoraTests
{
    private Calculadora _calculadora = null!;

    [SetUp] // Se ejecuta ANTES de cada test
    public void SetUp()
    {
        _calculadora = new Calculadora();
    }

    [Test] // Marca un método como test
    public void Sumar_DosNumeros_RetornaSuma()
    {
        // Arrange (preparar)
        int a = 2, b = 3;

        // Act (ejecutar)
        int resultado = _calculadora.Sumar(a, b);

        // Assert (verificar)
        Assert.That(resultado, Is.EqualTo(5));
    }

    [Test]
    public void Dividir_PorCero_LanzaExcepcion()
    {
        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => _calculadora.Dividir(10, 0));
    }
}
```

### Atributos de NUnit

| Atributo | Descripción |
|----------|-------------|
| `[TestFixture]` | Marca una clase como clase de test |
| `[Test]` | Marca un método como test |
| `[SetUp]` | Se ejecuta antes de cada test |
| `[TearDown]` | Se ejecuta después de cada test |
| `[OneTimeSetUp]` | Se ejecuta una vez al inicio de la clase |
| [OneTimeTearDown]` | Se ejecuta una vez al final de la clase |
| `[TestCase]` | Test parametrizado |
| `[TestCaseSource]` | TestCase con datos de una fuente externa |

### Tests parametrizados

```csharp
[TestFixture]
public class CalculadoraParametrosTests
{
    private Calculadora _calculadora = null!;

    [SetUp]
    public void SetUp()
    {
        _calculadora = new Calculadora();
    }

    [TestCase(2, 3, 5)]
    [TestCase(0, 0, 0)]
    [TestCase(-1, 1, 0)]
    [TestCase(10, -5, 5)]
    public void Sumar_VariosCasos_RetornaCorrecto(int a, int b, int esperado)
    {
        int resultado = _calculadora.Sumar(a, b);
        Assert.That(resultado, Is.EqualTo(esperado));
    }

    [TestCase(10, 2, 5)]
    [TestCase(9, 3, 3)]
    [TestCase(-10, 2, -5)]
    public void Dividir_VariosCasos_RetornaCorrecto(int a, int b, int esperado)
    {
        int resultado = _calculadora.Dividir(a, b);
        Assert.That(resultado, Is.EqualTo(esperado));
    }
}
```

### Organización con inner classes

```csharp
[TestFixture]
public class PersonaTests
{
    [TestFixture]
    public class CrearPersona
    {
        [Test]
        public void NombreValido_RetornaPersonaConNombre()
        {
            var persona = new Persona("Ana", "ana@email.com");
            Assert.That(persona.Nombre, Is.EqualTo("Ana"));
        }

        [Test]
        public void NombreVacio_LanzaArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Persona("", "ana@email.com"));
        }
    }

    [TestFixture]
    public class CalcularEdad
    {
        [Test]
        public void EdadPositiva_RetornaEdadCorrecta()
        {
            var persona = new Persona("Ana", "ana@email.com", 25);
            Assert.That(persona.Edad, Is.EqualTo(25));
        }

        [Test]
        public void EdadNegativa_LanzaArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Persona("Ana", "ana@email.com", -5));
        }
    }
}
```

> 💡 **Consejo:** Usa `SetUp` solo para inicializar objetos compartidos. No pongas lógica de negocio en `SetUp`: cada test debe ser independiente y autocontenido.

## 22.2. FluentAssertions: Aserciones Expresivas

**FluentAssertions** hace que las aserciones sean más legibles y proporciona mensajes de error descriptivos.

### Instalación

```bash
dotnet add package FluentAssertions
```

### Comparativa: NUnit vs FluentAssertions

```csharp
// Con NUnit (clásico)
Assert.That(resultado, Is.EqualTo(5));
Assert.That(lista, Is.Not.Empty);
Assert.That(nombre, Is.EqualTo("Ana").Or.EqualTo("Carlos"));

// Con FluentAssertions (expresivo)
resultado.Should().Be(5);
lista.Should().NotBeEmpty();
nombre.Should().BeOneOf("Ana", "Carlos");
```

### Aserciones comunes

```csharp
// Valores simples
resultado.Should().Be(5);
resultado.Should().NotBe(0);
resultado.Should().BeGreaterThan(0);
resultado.Should().BeInRange(1, 100);

// Colecciones
lista.Should().HaveCount(3);
lista.Should().Contain("Ana");
lista.Should().NotContain("Pedro");
lista.Should().BeInAscendingOrder();
lista.Should().OnlyContain(x => x > 0);

// Strings
nombre.Should().StartWith("An");
nombre.Should().EndWith("a");
nombre.Should().Contain("na");
nombre.Should().HaveLength(3);

// Objetos
persona.Should().NotBeNull();
persona.Nombre.Should().Be("Ana");
persona.Should().BeEquivalentTo(esperado);

// Excepciones
accion.Should().Throw<ArgumentException>()
    .WithMessage("*nombre*");
accion.Should().NotThrow();
```

### Aserciones con objetos complejos

```csharp
// BeEquivalentTo: comparar objetos ignorando propiedades no relevantes
var esperado = new Persona("Ana", "ana@email.com", 25);
var real = new Persona("Ana", "ana@email.com", 25);

real.Should().BeEquivalentTo(esperado);

// Ignorar propiedades específicas
real.Should().BeEquivalentTo(esperado, options => options
    .Ignoring(p => p.FechaRegistro)
    .Ignoring(p => p.Id));
```

> 📝 **Nota:** FluentAssertions genera mensajes de error como: `Expected persona.Nombre to be "Carlos", but "Ana" differs near index 0.` Mucho más descriptivo que un simple `Expected: 5, Actual: 4`.

## 22.3. Moq: Mocking de Interfaces

**Moq** crea implementaciones "falsas" de interfaces para aislar el componente que estás testeando. Si tu servicio depende de un repositorio, puedes mockear el repositorio para no depender de la base de datos real.

### Instalación

```bash
dotnet add package Moq
```

### Mockear un repositorio

```csharp
using Moq;

[TestFixture]
public class PersonaServiceTests
{
    private Mock<IPersonaRepository> _mockRepository = null!;
    private PersonaService _service = null!;

    [SetUp]
    public void SetUp()
    {
        // Crear el mock
        _mockRepository = new Mock<IPersonaRepository>();

        // Configurar el comportamiento del mock
        _mockRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Persona("Ana", "ana@email.com", 25));

        // Crear el servicio con el mock
        _service = new PersonaService(_mockRepository.Object);
    }

    [Test]
    public async Task ObtenerPorId_Existe_RetornaPersona()
    {
        // Act
        var resultado = await _service.ObtenerPorIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Ana");

        // Verify: comprobar que se llamó al repositorio
        _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Test]
    public async Task ObtenerPorId_NoExiste_RetornaNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Persona?)null);

        // Act
        var resultado = await _service.ObtenerPorIdAsync(999);

        // Assert
        resultado.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(999), Times.Once);
    }
}
```

### Setup con Match

```csharp
// Setup con condición
_mockRepository
    .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync((int id) => new Persona($"Usuario{id}", $"{id}@email.com", 25));

// Setup que lanza excepción
_mockRepository
    .Setup(r => r.GetByIdAsync(It.Is<int>(id => id < 0)))
    .ThrowsAsync(new ArgumentException("ID no puede ser negativo"));

// Verify con parámetros específicos
_mockRepository.Verify(
    r => r.AddAsync(It.Is<Persona>(p => p.Nombre == "Ana")),
    Times.Once);
```

### Mockear un servicio con resultado

```csharp
// Mockear un servicio que devuelve Result
var mockService = new Mock<IAcademiaService>();

mockService
    .Setup(s => s.ObtenerPorIdAsync(1))
    .ReturnsAsync(Result.Success(new Persona("Ana", "ana@email.com", 25)));

mockService
    .Setup(s => s.ObtenerPorIdAsync(999))
    .ReturnsAsync(Result.Failure<Persona>("Persona no encontrada"));
```

> 💡 **Consejo:** Usa `It.IsAny<T>()` para configurar "cualquier valor" y `It.Is<T>(condición)` para valores específicos. Usa `Times.Once`, `Times.Never`, `Times.AtLeastOnce` para verificar cuántas veces se llamó.

## 22.4. Patrón AAA: Arrange-Act-Assert

El patrón **AAA** (Arrange-Act-Assert) es la forma estándar de organizar un test:

```csharp
[Test]
public void CrearPedido_ProductoDisponible_RetornaPedidoConId()
{
    // ARRANGE: Preparar el escenario
    var producto = new Producto("Laptop", 999.99m);
    var cliente = new Cliente("Ana", "ana@email.com");
    var pedidoService = new PedidoService(_mockRepository.Object);

    // ACT: Ejecutar la operación a testear
    var resultado = pedidoService.CrearPedido(cliente, producto, cantidad: 1);

    // ASSERT: Verificar el resultado
    resultado.Should().NotBeNull();
    resultado.Id.Should().BeGreaterThan(0);
    resultado.Producto.Should().Be(producto);
    resultado.Cliente.Should().Be(cliente);
}
```

### AAA conMocks

```csharp
[Test]
public async Task Guardar_PedidoValido_GuardaEnRepositorio()
{
    // ARRANGE
    var mockRepo = new Mock<IPedidoRepository>();
    mockRepo
        .Setup(r => r.GuardarAsync(It.IsAny<Pedido>()))
        .ReturnsAsync((Pedido p) => p with { Id = 1 });

    var service = new PedidoService(mockRepo.Object);
    var pedido = new Pedido(clienteId: 1, productoId: 1, cantidad: 2);

    // ACT
    var resultado = await service.GuardarAsync(pedido);

    // ASSERT
    resultado.Should().NotBeNull();
    resultado.Id.Should().Be(1);

    mockRepo.Verify(
        r => r.GuardarAsync(It.Is<Pedido>(p =>
            p.ClienteId == 1 && p.ProductoId == 1 && p.Cantidad == 2)),
        Times.Once);
}
```

> ⚠️ **Advertencia:** Cada test debe ser **independiente**. No dependas del orden de ejecución ni de datos de otros tests. Usa `SetUp` para reiniciar el estado.

## 22.5. TestContainers: Tests con Docker

**TestContainers** lanza contenedores Docker reales para tests de integración. En vez de mockear una base de datos, usas una real (PostgreSQL, Redis, MongoDB) que se crea y destruye automáticamente.

### Instalación

```bash
dotnet add package Testcontainers
dotnet add package Testcontainers.PostgreSql
dotnet add package Testcontainers.Redis
```

### Test con PostgreSQL real

```csharp
using Testcontainers.PostgreSql;
using Npgsql;
using Dapper;

[TestFixture]
public class PersonaRepositoryIntegrationTests
{
    private PostgreSqlContainer _container = null!;
    private PersonaRepository _repository = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        // Lanzar contenedor PostgreSQL
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();

        // Crear la tabla
        using var connection = new NpgsqlConnection(_container.GetConnectionString());
        await connection.ExecuteAsync(@"
            CREATE TABLE Personas (
                Id SERIAL PRIMARY KEY,
                Nombre VARCHAR(100) NOT NULL,
                Email VARCHAR(200) NOT NULL,
                Edad INT NOT NULL,
                Activo BOOLEAN DEFAULT true
            )");

        _repository = new PersonaRepository(_container.GetConnectionString());
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _container.DisposeAsync();
    }

    [Test]
    public async Task Create_PersonaNueva_RetornaId()
    {
        // Arrange
        var persona = new Persona { Nombre = "Ana", Email = "ana@email.com", Edad = 25 };

        // Act
        int id = await _repository.CreateAsync(persona);

        // Assert
        id.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GetAll_ConDatos_RetornaTodas()
    {
        // Arrange
        await _repository.CreateAsync(new Persona { Nombre = "Ana", Email = "a@a.com", Edad = 25 });
        await _repository.CreateAsync(new Persona { Nombre = "Carlos", Email = "c@c.com", Edad = 30 });

        // Act
        var personas = (await _repository.GetAllAsync()).ToList();

        // Assert
        personas.Should().HaveCount(2);
        personas.Should().Contain(p => p.Nombre == "Ana");
        personas.Should().Contain(p => p.Nombre == "Carlos");
    }

    [Test]
    public async Task GetById_Existe_RetornaPersona()
    {
        // Arrange
        var persona = new Persona { Nombre = "Ana", Email = "ana@email.com", Edad = 25 };
        int id = await _repository.CreateAsync(persona);

        // Act
        var resultado = await _repository.GetByIdAsync(id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nombre.Should().Be("Ana");
    }
}
```

### Test con Redis real

```csharp
using Testcontainers.Redis;

[TestFixture]
public class RedisCacheServiceTests
{
    private RedisContainer _container = null!;
    private RedisCacheService _cache = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _container = new RedisBuilder()
            .WithImage("redis:7")
            .Build();

        await _container.StartAsync();
        _cache = new RedisCacheService(_container.GetConnectionString());
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _container.DisposeAsync();
    }

    [Test]
    public async Task SetAsync_ClaveValor_LuegoGetAsyncLoRetorna()
    {
        // Arrange & Act
        await _cache.SetAsync("test:key", "test_value");

        // Assert
        var resultado = await _cache.GetAsync("test:key");
        resultado.Should().Be("test_value");
    }
}
```

> 💡 **Consejo:** Los tests con TestContainers son más lentos que los unitarios (arrancan Docker), pero mucho más fiables. Úsalos para tests de integración donde necesitas una base de datos real.

### 22.5.1. Coverlet: Cobertura de Código

¿Cómo sabes si tus tests cubren todo el código que deberían? Con **coverlet** — una herramienta que mide qué porcentaje de tu código se ejecuta al correr los tests.

#### Instalación

En el **proyecto de test** (.Test.csproj):

```xml
<ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
</ItemGroup>
```

O desde consola:

```bash
dotnet add package coverlet.collector
```

#### Generar informe de cobertura

```bash
# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# El informe se genera en:
# MiProyecto.Test/TestResults/<GUID>/coverage.cobertura.xml
```

#### Formato del informe (Cobertura XML)

El archivo `coverage.cobertura.xml` contiene datos como:

```xml
<line_rate="0.85" />  <!-- 85% de cobertura de líneas -->
<branch_rate="0.78" /> <!-- 78% de cobertura de ramas -->
<class name="MiProyecto.Services.ProductoService" 
        line-rate="1.0" />  <!-- 100% en esta clase -->
```

#### Ver cobertura en HTML (opcional)

```bash
# Instalar ReportGenerator
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generar informe HTML
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage

# Abrir en navegador
start coverage/index.html
```

#### Excluir código de la cobertura

No todo el código debe medirse. Por ejemplo, `Program.cs` con Top Level Statements es difícil de cubrir. Exclúyelo en `.runsettings`:

```xml
<!-- coverlet.runsettings -->
<RunSettings>
  <DataCollectionRunSettings>
    <DataCollectors>
      <DataCollector friendlyName="XPlat code coverage">
        <Configuration>
          <Exclude>[Program]*</Exclude>
          <Exclude>[*]*.Models.*</Exclude>
        </Configuration>
      </DataCollector>
    </DataCollectors>
  </DataCollectionRunSettings>
</RunSettings>
```

Y ejecútalo así:

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

#### Qué significa un buen porcentaje

| Cobertura | Significado | Acción |
|-----------|-------------|--------|
| **90-100%** | Excelente | Mantener |
| **80-89%** | Buena | Revisar qué falta |
| **70-79%** | Aceptable | Añadir tests faltantes |
| **< 70%** | Insuficiente | Riesgo de bugs no detectados |

> ⚠️ **Advertencia:** Alta cobertura **no** garantiza calidad. Puedes tener 100% de cobertura con tests que no verifican nada. La cobertura es una **métrica**, no un objetivo final.

> 💡 **Consejo para el examen:** Si un método tiene un `if/else` con dos ramas, necesitas al menos dos tests para cubrir ambas ramas. Coverlet detecta esto — si solo cubres una rama, te lo dice.

### Comparativa de tipos de test

| Tipo | Velocidad | Fiabilidad | Dependencias | Ejemplo |
|------|-----------|------------|-------------|---------|
| **Unitario** | ⚡ Muy rápido | Media | Mocks | `PersonaService` aíslalo del repositorio |
| **Integración** | 🐢 Medio | Alta | TestContainers | `PersonaRepository` con PostgreSQL real |
| **E2E** | 🐌 Lento | Muy alta | App completa | Un usuario real haciendo login |

---

**Resumen del punto:**

| Concepto | Descripción |
|----------|-------------|
| **NUnit** | Framework de tests con `[TestFixture]`, `[Test]`, `[SetUp]` |
| **\[TestCase\]** | Tests parametrizados con múltiples entradas |
| **FluentAssertions** | Aserciones legibles: `resultado.Should().Be(5)` |
| **Moq** | Crear mocks de interfaces: `Setup`, `Verify` |
| **AAA** | Arrange-Act-Assert: el patrón de organización de tests |
| **TestContainers** | Contenedores Docker reales para tests de integración |
| **Unit test** | Rápido, aísla componentes, usa mocks |
| **Integration test** | Prueba componentes juntos, usa BD real |

En el siguiente punto veremos Docker: qué son los contenedores, cómo crear imágenes con Dockerfile y orquestar servicios con Docker Compose.

## 22.6. Depuración de Errores en C#

> 💡 **Punto de partida:** Ves un error en rojo y no sabes por dónde empezar. No es para entrar en pánico — es para leer. Un stack trace bien leído te dice exactamente dónde está el problema. Vamos a aprender a leerlo.

### 22.6.1. Leer un Stack Trace

Cuando tu programa lanza una excepción, ves algo así:

```
System.InvalidOperationException: Unable to resolve service for type 'IUserService'
   at Microsoft.Extensions.DependencyInjection.ServiceLookup...
   at Program.<Main>$(String[] args) in C:\Proyecto\Program.cs:line 27
```

**Cómo leerlo de abajo a arriba:**

| Línea | Qué te dice |
|-------|-------------|
| `Program.cs:line 27` | **Dónde** se rompió (fichero + línea) |
| `Unable to resolve service for type 'IUserService'` | **Qué** falla (no encuentra IUserService) |
| `at Microsoft.Extensions...` | **Por qué** falla (el contenedor de DI no puede resolverlo) |

📌 **Ejemplo real:** Es como un GPS de errores — te dice el destino (fichero), la calle (línea) y por qué no puedes llegar (la causa raíz).

> 💡 **Truco:** Busca SIEMPRE la **última línea** de tu código en el stack trace. Las líneas anteriores son de librerías internas — ahí no está tu error.

### 22.6.2. Tipos de Error Comunes en C#

| Error | Causa típica | Solución |
|-------|-------------|----------|
| `NullReferenceException` | Accedes a `.Algo` de un null | Usa `?.` o `??` o `if (x is not null)` |
| `InvalidOperationException` | DI no puede resolver un servicio | Registra el servicio en `ConfigureServices` |
| `FileNotFoundException` | Falta un fichero de configuración | Comprueba la ruta en `AppContext.BaseDirectory` |
| `TaskCanceledException` | Timeout o CancellationToken activado | Revisa tiempos o tokens |
| `JsonException` | JSON mal formado | Usa [jsonlint.com](https://jsonlint.com) para validar |

### 22.6.3. Herramientas de Depuración

**En el IDE (Rider / VS Code):**

```
1. Breakpoints: Haz clic en el margen izquierdo de la línea
2. F5 / Debug: Ejecuta hasta el breakpoint
3. F10 / Step Over: Ejecuta la siguiente línea
4. F11 / Step Into: Entra en la función
5. F8 / Step Out: Sale de la función actual
6. Variables: Ve el valor de las variables en el panel
```

**Desde consola:**

```bash
# Ejecutar con depurador adjunto
dotnet run --debug

# Ver exception completa (útil en logs)
dotnet run -- 2>&1 | Out-File error.log
```

**`dotnet-trace` (diagnóstico avanzado):**

```bash
# Instalar herramienta
dotnet tool install --global dotnet-trace

# Capturar trace de un proceso
dotnet-trace collect --process-id <PID>
```

> 💡 **Consejo para el examen:** Si te sale un error que no entiendes, **lee el mensaje completo**. Los errores de C# suelen decir exactamente qué falla y dónde. No mires solo el tipo de excepción — lee la descripción.

### 22.6.4. Depuración de Tests

Cuando un test falla, NUnit te muestra:

```
Failed Assert.That(resultado).Should().Be(5)
  Expected: 5
  But was:  4
```

**Cómo depurar un test:**

```csharp
// 1. Añade un breakpoint en el test
// 2. En Rider: Click derecho → Debug 'Tests'
// 3. En VS Code: pestaña Testing → Debug Test

// O usa Console.WriteLine para debug rápido:
var resultado = service.Calcular(2, 3);
Console.WriteLine($"DEBUG: resultado = {resultado}"); // Mira en Output
resultado.Should().Be(5);
```

> ⚠️ **Advertencia:** `Console.WriteLine` en tests es solo para depuración temporal. Los tests deben ser autoexplicativos — si necesitas prints para entender qué falla, el test necesita mejor nombre o estructura.

## 22.7. Regla: Nada se Entrega sin Tests

> ⚠️ **Regla CRÍTICA:** En este módulo, **no se acepta ninguna entrega sin tests** y **sin informe de cobertura de código**. Esto no es una opción — es lo mínimo esperable de cualquier desarrollador profesional.

### Por qué es obligatorio

| Sin tests | Con tests |
|-----------|-----------|
| "Funciona en mi máquina" | "Funciona siempre" |
| Miedo a cambiar código | Confianza para refactorizar |
| Bugs que aparecen en producción | Bugs atrapados antes de desplegar |
| No sabes si tu cobertura es buena | Sabes exactamente qué cubres |

📌 **Ejemplo real:** En cualquier empresa seria, un PR (Pull Request) sin tests no se merge. Punto. Si tu código no tiene tests, no está terminado.

### Qué incluir en cada entrega

```
MiProyecto/
├── MiProyecto/           # Código fuente
└── MiProyecto.Test/      # Tests
    └── ...
```

**Al entregar, incluye SIEMPRE:**

1. **Los tests pasan:** `dotnet test` → todos en verde
2. **Informe de cobertura:** `dotnet test --collect:"XPlat Code Coverage"`
3. **Mínimo aceptable:** 80% de cobertura en código de negocio

### Cómo generar el informe de cobertura

```bash
# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# El informe se genera en:
# MiProyecto.Test/TestResults/<GUID>/coverage.cobertura.xml

# Para verlo en HTML (opcional, requiere ReportGenerator):
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage
# Abrir coverage/index.html en el navegador
```

### Reglas de cobertura

| Código | Cobertura mínima | Ejemplo |
|--------|-----------------|---------|
| **Modelos/Records** | 100% | Properties, constructores, métodos de dominio |
| **Services** | 90%+ | Lógica de negocio, validaciones |
| **Repositories** | 80%+ | CRUD, consultas |
| **Utilidades** | 80%+ | Helpers, extensiones |

> 💡 **Consejo:** La cobertura no lo es todo — un test que solo hace `Assert.Pass()` no cubre nada. Pero sin cobertura, no tienes ni idea de qué partes de tu código están protegidas.

---

**Resumen del punto:**

| Concepto | Descripción |
|----------|-------------|
| **Stack Trace** | Léelo de abajo a arriba — la línea de tu código es la última |
| **Breakpoints** | Pausa la ejecución para inspeccionar variables |
| **dotnet-trace** | Diagnóstico avanzado desde consola |
| **Tests obligatorios** | No se entrega nada sin tests verdes |
| **Cobertura** | Mínimo 80% en código de negocio, informe incluido |
