**Test: Desarrollo Web en Entorno Servidor**

**Instrucciones:** Lee cada pregunta cuidadosamente y selecciona la opción que consideres correcta.

---

## PARTE 1 (Temas 01-10)

**Tema 01: Introducción al Desarrollo Web en Servidor**

1.  ¿Qué es el desarrollo web en entorno servidor?
    A) Crear páginas estáticas con HTML y CSS únicamente
    B) Desarrollar la parte de la aplicación que se ejecuta en el servidor, gestionando datos y lógica de negocio
    C) Programar únicamente el diseño visual que ve el usuario
    D) Configurar routers y switches de red

2.  ¿Cuál de estos NO es un objetivo del despliegue web?
    A) Accesibilidad
    B) Estabilidad
    C) Estilismo visual
    D) Seguridad

3.  ¿Qué componente se encarga de la lógica de negocio en una aplicación web?
    A) Front-end
    B) Back-end
    C) CSS
    D) HTML

4.  ¿Qué diferencia hay entre Front-end y Back-end?
    A) El Front-end se ejecuta en el servidor y el Back-end en el navegador
    B) El Front-end maneja la interfaz visual y el Back-end la lógica y los datos
    C) No hay diferencia, son sinónimos
    D) El Front-end solo usa Java y el Back-end solo usa Python

5.  ¿Qué tecnologías forman el ecosistema completo de una aplicación web?
    A) Solo HTML
    B) HTML en el cliente, un lenguaje de programación en el servidor y una base de datos
    C) únicamente bases de datos relacionales
    D) Exclusivamente lenguajes de scripting del lado del cliente

**Tema 02: Componentes de una Web**

6.  ¿Cuáles son las tres tecnologías fundamentales del Front-end?
    A) Java, C#, Python
    B) HTML, CSS, JavaScript
    C) SQL, NoSQL, Redis
    D) Docker, Kubernetes, AWS

7.  ¿Qué lenguaje se utiliza para estructurar el contenido de una página web?
    A) CSS
    B) JavaScript
    C) HTML
    D) Python

8.  ¿Qué tecnología se usa para dar estilos y apariencia visual a una página web?
    A) HTML
    B) JavaScript
    C) CSS
    D) SQL

9.  ¿Cuál es la función principal de JavaScript en una aplicación web?
    A) Estructurar el contenido
    B) Dar estilos visuales
    C) Añadir interactividad y comportamiento dinámico
    D) Conectar con la base de datos

10. ¿Qué tipo de contenido genera el navegador a partir del HTML, CSS y JavaScript?
    A) Un ejecutable de escritorio
    B) Una página web renderizada que el usuario puede visualizar e interactuar
    C) Un fichero de base de datos
    D) Un componente del servidor

**Tema 03: Arquitecturas Web**

11. ¿Qué arquitectura divide una aplicación en capas como Presentación, Negocio y Datos?
    A) Arquitectura de microservicios
    B) Arquitectura en capas (N-Tier)
    C) Arquitectura serverless
    D) Arquitectura peer-to-peer

12. ¿Qué ventaja principal ofrecen los microservicios frente a una arquitectura monolítica?
    A) Son más fáciles de desarrollar al principio
    B) Permiten escalar y desplegar componentes de forma independiente
    C) Requieren menos memoria
    D) No necesitan bases de datos

13. ¿En qué consiste la arquitectura serverless?
    A) No existe el servidor
    B) El desarrollador gestiona los servidores manualmente
    C) El proveedor gestiona la infraestructura y el desarrollador solo despliega código
    D) Solo funciona con bases de datos SQL

14. ¿Qué es un monolito en arquitectura web?
    A) Una aplicación donde toda la lógica está en un único componente desplegable
    B) Una base de datos con una sola tabla
    C) Un servidor sin conexión a internet
    D) Un tipo de protocolo de red

15. ¿Qué arquitectura usa un modelo cliente-servidor donde el cliente se comunica con el servidor mediante peticiones HTTP?
    A) Peer-to-peer
    B) Cliente-Servidor
    C) Mainframe
    D) Bitácora

**Tema 04: Protocolo HTTP**

16. ¿Qué significa HTTP?
    A) HyperText Transfer Protocol
    B) High Tech Transfer Protocol
    C) Hyper Transfer Text Protocol
    D) Home Tool Transfer Protocol

17. ¿Qué característica define a HTTP como protocolo "sin estado"?
    A) No puede transmitir datos
    B) Cada petición es independiente, el servidor no recuerda peticiones anteriores
    C) No admite cabeceras
    D) Solo funciona con conexiones seguras

18. ¿Qué verbo HTTP se utiliza para crear un nuevo recurso en el servidor?
    A) GET
    B) POST
    C) DELETE
    D) OPTIONS

19. ¿Qué código de estado HTTP indica que el recurso solicitado no fue encontrado?
    A) 200
    B) 301
    C) 404
    D) 500

20. ¿Qué verbo HTTP se utiliza para actualizar un recurso existente?
    A) GET
    B) POST
    C) PUT
    D) PATCH

**Tema 05: Servicios Web y APIs**

21. ¿Qué es una API REST?
    A) Un tipo de base de datos
    B) Una interfaz de programación que usa verbos HTTP para comunicar clientes y servidores
    C) Un lenguaje de programación
    D) Un navegador web

22. ¿Qué formato de datos es el más utilizado en las APIs REST modernas?
    A) XML
    B) CSV
    C) JSON
    D) YAML

23. ¿Qué verbo HTTP se asocia típicamente con la operación CRUD de "Leer"?
    A) POST
    B) GET
    C) PUT
    D) DELETE

24. ¿Qué es un endpoint en una API?
    A) Un tipo de servidor
    B) Una URL específica que expone un recurso o servicio
    C) Un cliente que consume datos
    D) Un protocolo de comunicación

25. ¿Qué diferencia hay entre SOAP y REST?
    A) SOAP es más ligero que REST
    B) REST usa XML exclusivamente y SOAP usa JSON
    C) REST es más flexible y ligero, SOAP es más estricto y estandarizado
    D) No hay diferencia significativa

**Tema 06: Web Dinámica**

26. ¿Qué es la programación del lado del servidor (server-side)?
    A) Programar exclusivamente con HTML
    B) Ejecutar código en el servidor para generar respuestas dinámicas
    C) Ejecutar JavaScript en el navegador
    D) Diseñar hojas de estilo

27. ¿Qué tecnología permite generar contenido HTML dinámico directamente en el servidor en .NET?
    A) Razor Pages
    B) CSS Grid
    C) JavaScript vanilla
    D) jQuery

28. ¿Qué es AJAX?
    A) Un lenguaje de programación
    B) Una técnica para enviar y recibir datos asíncronamente sin recargar la página
    C) Un framework de CSS
    D) Un protocolo de red

29. ¿Qué diferencian a una web estática de una web dinámica?
    A) La web estática tiene más páginas
    B) La web dinámica genera contenido en tiempo de ejecución según el usuario o contexto
    C) La web dinámica no necesita servidor
    D) No hay diferencia real

30. ¿Qué ventaja ofrece el server-side rendering (SSR) sobre el client-side rendering?
    A) Es más sencillo de implementar
    B) El contenido se entrega pre-renderizado, mejorando SEO y tiempo de carga inicial
    C) No necesita servidor web
    D) Funciona sin conexión a internet

**Tema 07: Lenguajes y Frameworks**

31. ¿Qué es un framework web?
    A) Un tipo de base de datos
    B) Un conjunto de herramientas y bibliotecas que facilitan el desarrollo de aplicaciones web
    C) Un navegador especializado
    D) Un protocolo de red

32. ¿Qué lenguaje de programación es el principal en el ecosistema .NET para desarrollo web?
    A) Java
    B) Python
    C) C#
    D) PHP

33. ¿Qué framework de Microsoft se usa para desarrollar aplicaciones web en .NET?
    A) Spring Boot
    B) Django
    C) ASP.NET Core
    D) Laravel

34. ¿Qué ventaja ofrece usar un framework frente a programar desde cero?
    A) El framework elimina la necesidad de escribir código
    B) Ofrece componentes reutilizables, seguridad y buenas prácticas predefinidas
    C) No es necesario entender el lenguaje de programación
    D) Garantiza que la aplicación no tenga errores

35. ¿Cuál es la principal diferencia entre un lenguaje interpretado y uno compilado?
    A) El interpretado es siempre más rápido
    B) El compilado genera un ejecutable antes de ejecutarse, el interpretado se ejecuta línea a línea
    C) El interpretado solo sirve para web
    D) No hay diferencia real

**Tema 08: Servidores Web**

36. ¿Qué es un servidor web?
    A) Un tipo de base de datos
    B) Un software que recibe peticiones HTTP y devuelve respuestas
    C) Un navegador modificado
    D) Un dispositivo de almacenamiento

37. ¿Qué servidor web viene integrado por defecto en ASP.NET Core?
    A) Apache
    B) Nginx
    C) Kestrel
    D) IIS Express

38. ¿Qué es Kestrel en el ecosistema .NET?
    A) Un servidor web de alto rendimiento que viene integrado en ASP.NET Core
    B) Un ORM para bases de datos
    C) Un framework de JavaScript
    D) Un cliente HTTP

39. ¿En qué se diferencia Kestrel de servidores como Apache o Nginx?
    A) Kestrel no puede manejar peticiones HTTP
    B) Kestrel está optimizado para .NET pero normalmente se usa detrás de un reverse proxy
    C) Kestrel solo funciona en Linux
    D) No hay diferencia funcional

40. ¿Qué es un reverse proxy en el contexto de despliegue web?
    A) Un servidor que ejecuta la aplicación directamente
    B) Un servidor intermedio que reenvía peticiones al servidor de aplicación y gestiona SSL, balanceo y caché
    C) Un tipo de base de datos
    D) Un navegador web

**Tema 09: Despliegue**

41. ¿Qué es Docker en el contexto del despliegue?
    A) Un lenguaje de programación
    B) Una plataforma para crear y ejecutar aplicaciones en contenedores
    C) Un tipo de base de datos
    D) Un navegador web

42. ¿Qué es un contenedor Docker?
    A) Una máquina virtual completa
    B) Un entorno aislado que empaqueta la aplicación con todas sus dependencias
    C) Un tipo de servidor web
    D) Un protocolo de red

43. ¿Qué diferencia hay entre una imagen y un contenedor Docker?
    A) No hay diferencia
    B) La imagen es una plantilla inmutable, el contenedor es una instancia en ejecución
    C) El contenedor es la plantilla y la imagen es la ejecución
    D) La imagen solo funciona en Linux y el contenedor en Windows

44. ¿Qué es Docker Compose?
    A) Un lenguaje de programación
    B) Una herramienta para definir y ejecutar aplicaciones con múltiples contenedores
    C) Un servidor web
    D) Un cliente de bases de datos

45. ¿Qué es un volumen en Docker?
    A) Un tipo de red
    B) Un mecanismo para persistir datos fuera del ciclo de vida del contenedor
    C) Una variable de entorno
    D) Un archivo de configuración

**Tema 10: Seguridad y Monitorización**

46. ¿Qué es HTTPS?
    A) Una versión de HTTP con encriptación TLS/SSL para asegurar la comunicación
    B) Un protocolo para enviar correos electrónicos
    C) Un tipo de base de datos
    D) Un lenguaje de programación

47. ¿Qué es un token JWT?
    A) Un tipo de contraseña
    B) Un estándar para transmitir información de forma segura entre dos partes como objeto JSON
    C) Un protocolo de red
    D) Un framework de testing

48. ¿Qué es el CORS en el contexto de la seguridad web?
    A) Un tipo de ataque
    B) Un mecanismo que permite o restringe peticiones de diferentes orígenes
    C) Un protocolo de autenticación
    D) Un tipo de base de datos

49. ¿Qué es la monitorización de aplicaciones web?
    A) Vigilar físicamente el servidor
    B) Recopilar y analizar métricas, logs y trazas para detectar problemas
    C) Configurar el firewall
    D) Actualizar el sistema operativo

50. ¿Qué finalidad tiene un sistema de logs estructurados en una aplicación web?
    A) Almacenar datos de usuario
    B) Registrar eventos con formato predefinido para facilitar la búsqueda, el filtrado y el análisis de problemas
    C) Comprimir ficheros de backup
    D) Generar código HTML automáticamente

---

## PARTE 2 (Temas 11-25)

**Tema 11: Inyección de Dependencias**

51. ¿Qué problema resuelve la Inyección de Dependencias?
    A) La lentitud del servidor
    B) El acoplamiento duro entre clases que crean sus propias dependencias
    C) Los errores de compilación
    D) La falta de estilos CSS

52. ¿Qué principio SOLID se relaciona directamente con la Inyección de Dependencias?
    A) Single Responsibility
    B) Open/Closed
    C) Dependency Inversion
    D) Interface Segregation

53. ¿Qué ciclo de vida en DI crea una nueva instancia cada vez que se resuelve el servicio?
    A) Singleton
    B) Scoped
    C) Transient
    D) Per Request

54. ¿Qué ciclo de vida mantiene una única instancia durante toda la vida de la aplicación?
    A) Transient
    B) Scoped
    C) Singleton
    D) Per Request

**Tema 12: Patrones y Arquitecturas**

55. ¿Qué patrón de diseño encapsula la lógica de negocio y coordina las operaciones entre diferentes componentes?
    A) Repository
    B) Service
    C) Factory
    D) Singleton

56. ¿Qué patrón encapsula el acceso a datos, abstrayendo la fuente de datos?
    A) Service
    B) Repository
    C) Observer
    D) Strategy

57. ¿Qué patrón de diseño crea objetos sin especificar la clase exacta del objeto que se creará?
    A) Singleton
    B) Repository
    C) Factory
    D) Observer

58. ¿Qué patrón arquitectónico separa la aplicación en Modelo, Vista y Controlador?
    A) Repository
    B) MVC
    C) Factory
    D) Singleton

**Tema 13: LINQ y DataFrames**

59. ¿Qué es LINQ?
    A) Un tipo de base de datos
    B) Un lenguaje de consultas integrado en C# para trabajar con colecciones de datos
    C) Un framework de testing
    D) Un protocolo de red

60. ¿Qué método LINQ se usa para filtrar elementos que cumplen una condición?
    A) Select
    B) Where
    C) OrderBy
    D) GroupBy

61. ¿Qué es un DataFrame en C#?
    A) Un tipo de base de datos relacional
    B) Una estructura de datos tabular similar a una tabla SQL o Excel
    C) Un protocolo de comunicación
    D) Un framework de testing

62. ¿Qué paquete NuGet se necesita para usar DataFrames en C#?
    A) Microsoft.Data.Analysis
    B) System.Dataframe
    C) CsvHelper
    D) EntityFramework

**Tema 14: Ficheros y Formatos**

63. ¿Qué formato de fichero es ideal para almacenar configuraciones de una aplicación .NET?
    A) .exe
    B) appsettings.json
    C) .dll
    D) .tmp

64. ¿Qué librería se usa en C# para trabajar con ficheros CSV de forma eficiente?
    A) CsvHelper
    B) CsvNet
    C) FileHelper
    D) CsvSharp

65. ¿Qué namespace de .NET se usa para operaciones básicas con ficheros?
    A) System.Data
    B) System.IO
    C) System.Net
    D) System.XML

**Tema 15: Result/ROP**

66. ¿Qué problema principal resuelve el patrón Result (ROP)?
    A) La lentitud del servidor
    B) Manejar errores de dominio sin usar excepciones para control de flujo
    C) La falta de estilos CSS
    D) Los errores de compilación

67. ¿Qué tipo genérico representa el resultado de una operación que puede tener éxito o fallo?
    A) Result<T, TError>
    B) Option<T>
    C) Either<L, R>
    D) Try<T>

68. ¿Qué librería de C# implementa el patrón Result con Bind, Map, Ensure, Tap, Match?
    A) FluentAssertions
    B) CSharpFunctionalExtensions
    C) Newtonsoft.Json
    D) AutoMapper

69. ¿Qué tipo representa la presencia o ausencia de un valor sin usar null?
    A) Result<T>
    B) Maybe<T>
    C) Optional<T>
    D) Nullable<T>

**Tema 16: Concurrencia y Asincronía**

70. ¿Qué palabra clave en C# se usa para ejecutar código de forma asíncrona?
    A) sync
    B) async/await
    C) parallel
    D) thread

71. ¿Qué problema resuelve el async/await en aplicaciones web?
    A) La falta de memoria
    B) Bloquear el hilo del servidor durante operaciones de larga duración
    C) Los errores de compilación
    D) La falta de estilos

72. ¿Qué diferencia hay entre concurrencia y paralelismo?
    A) No hay diferencia
    B) Concurrencia maneja múltiples tareas progresivamente, paralelismo ejecuta tareas simultáneamente
    C) Paralelismo es para un solo hilo
    D) Concurrencia solo funciona con bases de datos

73. ¿Qué tipo de retorno se usa en métodos asíncronos en C#?
    A) void
    B) Task<T> o ValueTask<T>
    C) string
    D) object

**Tema 17: Programación Reactiva**

74. ¿Qué es la Programación Reactiva?
    A) Un tipo de base de datos
    B) Un paradigma basado en flujos de datos y propagación de cambios
    C) Un framework de CSS
    D) Un protocolo de red

75. ¿Qué tipo representa un flujo de datos que emite valores a lo largo del tiempo?
    A) List<T>
    B) Observable<T>
    C) Array<T>
    D) Dictionary<K,V>

76. ¿Qué operador Rx permite filtrar elementos de un observable según una condición?
    A) Select
    B) Where
    C) Merge
    D) CombineLatest

**Tema 18: Consumo de APIs con Refit**

77. ¿Qué es Refit?
    A) Un framework de testing
    B) Una librería que convierte interfaces en clientes HTTP tipados
    C) Un ORM para bases de datos
    D) Un servidor web

78. ¿Qué característica principal ofrece Refit frente a usar HttpClient manualmente?
    A) Es más lento
    B) Elimina código boilerplate y ofrece tipado en tiempo de compilación
    C) No requiere interfaces
    D) Solo funciona con JSON

79. ¿Qué atributo se usa en Refit para definir un endpoint HTTP en una interfaz?
    A) [HttpGet], [HttpPost], etc.
    B) [Route]
    C) [ApiController]
    D) [ServiceFilter]

**Tema 19: Configuration y Logging/Serilog**

80. ¿Qué archivo se usa para almacenar la configuración de una aplicación ASP.NET Core?
    A) config.xml
    B) appsettings.json
    C) settings.ini
    D) config.yaml

81. ¿Qué librería de logging se usa frecuentemente en .NET por su potencia y flexibilidad?
    A) Log4Net
    B) NLog
    C) Serilog
    D) Console.WriteLine

82. ¿Qué es un "sink" en Serilog?
    A) Un tipo de base de datos
    B) Un destino donde se envían los logs (consola, fichero, base de datos)
    C) Un filtro de logs
    D) Un nivel de log

**Tema 20: Entity Framework Core**

83. ¿Qué es un ORM?
    A) Un tipo de base de datos
    B) Una herramienta que mapea entre clases C# y tablas de base de datos
    C) Un servidor web
    D) Un framework de testing

84. ¿Qué clase en EF Core representa una sesión con la base de datos?
    A) DbContext
    B) Repository
    C) Service
    D) Controller

85. ¿Qué diferencia hay entre Data Annotations y Fluent API para configurar modelos en EF Core?
    A) No hay diferencia
    B) Data Annotations usa atributos en la clase, Fluent API usa código en OnModelCreating
    C) Fluent API solo funciona con SQL Server
    D) Data Annotations es más potente que Fluent API

86. ¿Qué operación se usa para aplicar cambios de esquema en la base de datos con EF Core?
    A) Update-Database
    B) Migrate
    C) Sync
    D) Apply

**Tema 21: SQL vs NoSQL**

87. ¿Qué tipo de base de datos SQL utiliza tablas con esquema fijo?
    A) MongoDB
    B) PostgreSQL
    C) Redis
    D) Cassandra

88. ¿Qué base de datos NoSQL almacena documentos en formato JSON/BSON?
    A) PostgreSQL
    B) MySQL
    C) MongoDB
    D) SQL Server

89. ¿Qué tipo de NoSQL es ideal para datos de sesión y caché de alta velocidad?
    A) MongoDB
    B) Redis
    C) PostgreSQL
    D) Cassandra

90. ¿Cuándo es preferible usar una base de datos SQL frente a NoSQL?
    A) Cuando necesitas escalabilidad horizontal masiva
    B) Cuando necesitas transacciones ACID y integridad referencial
    C) Cuando trabajas exclusivamente con documentos JSON
    D) Cuando no necesitas persistir datos

**Tema 22: Testing Avanzado**

91. ¿Qué es un test unitario?
    A) Un test que verifica toda la aplicación de punta a punta
    B) Un test que verifica un componente aislado del sistema
    C) Un test que verifica la interfaz gráfica
    D) Un test que verifica la base de datos

92. ¿Qué framework de testing se usa comúnmente en proyectos .NET?
    A) JUnit
    B) NUnit
    C) Mocha
    D) Jasmine

93. ¿Qué es un mock en el contexto de testing?
    A) Una base de datos de prueba
    B) Un objeto falso que simula el comportamiento de una dependencia real
    C) Un tipo de test de integración
    D) Un framework de testing

**Tema 23: Docker**

94. ¿Qué instrucción se usa en un Dockerfile para especificar la imagen base?
    A) RUN
    B) COPY
    C) FROM
    D) CMD

95. ¿Qué es una multi-stage build en Docker?
    A) Un contenedor con múltiples aplicaciones
    B) Un Dockerfile que usa varias fases para reducir el tamaño de la imagen final
    C) Un tipo de volumen
    D) Un comando de Docker Compose

96. ¿Qué archivo se usa para definir y ejecutar múltiples servicios Docker?
    A) Dockerfile
    B) docker-compose.yml
    C) .dockerignore
    D) Dockerfile.multistage

**Tema 24: Seguridad**

97. ¿Qué algoritmo se recomienda para hashear contraseñas en aplicaciones web?
    A) MD5
    B) SHA-1
    C) BCrypt
    D) Base64

98. ¿Qué código de estado HTTP se devuelve cuando un usuario no está autenticado?
    A) 403 Forbidden
    B) 401 Unauthorized
    C) 404 Not Found
    D) 500 Internal Server Error

**Tema 25: Resumen**

99. ¿Cuáles son las dos partes fundamentales de la Unidad 01?
    A) Front-end y Back-end
    B) Fundamentos del Desarrollo Web en Servidor y C# Avanzado para Desarrollo Servidor
    C) HTTP y HTTPS
    D) SQL y NoSQL

100. ¿Cuál es el siguiente paso después de completar la Unidad 01?
    A) Aprender HTML y CSS desde cero
    B) Desarrollo de servicios web en .NET (Unidad 02)
    C) Instalar un sistema operativo nuevo
    D) Aprender un lenguaje de programación diferente
