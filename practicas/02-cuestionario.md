# Cuestionario de Desarrollo de Respuestas: Desarrollo Web en Entorno Servidor

**Instrucciones:** Responde cada pregunta de forma clara y concisa. Puedes usar ejemplos de código si es necesario.

## PARTE 1 (Temas 01-10)

1.  **¿Qué es el desarrollo web en entorno servidor y por qué es importante en la arquitectura de una aplicación web moderna?**

2.  **Explica la diferencia entre Front-end y Back-end. ¿Qué es el "Back-end universal" y qué ventajas aporta?**

3.  **Describe brevemente la arquitectura MVC. ¿Cómo se relaciona con el principio SOLID? ¿En qué se diferencia una arquitectura monolítica de una basada en microservicios?**

4.  **¿Cuáles son los verbos HTTP más utilizados? Menciona al menos 5 códigos de estado HTTP y explica qué significan. ¿Cuál es la diferencia principal entre HTTP y HTTPS?**

5.  **Compara REST, GraphQL y WebSocket. ¿Cuándo usarías cada uno de estos paradigmas de comunicación en una aplicación?**

6.  **¿Qué es la web dinámica? Explica la diferencia entre renderizado en servidor (SSR) y aplicaciones de página única (SPA). Menciona al menos 2 tecnologías de servidor para desarrollar web dinámica.**

7.  **Explica la diferencia entre lenguajes de scripting, compilados y de bytecode. Pon un ejemplo de cada uno en el contexto del desarrollo web.**

8.  **¿Qué son Apache, Nginx y Kestrel? ¿Para qué se utiliza cada uno y en qué se diferencian?**

9.  **¿Qué es Docker y por qué es útil para el despliegue de aplicaciones? Explica brevemente qué es CI/CD y cómo contribuye a la escalabilidad.**

10. **Explica la diferencia entre autenticación y autorización. ¿Qué es JWT y cómo funciona? ¿Por qué son importantes los logs en la seguridad de una aplicación?**

## PARTE 2 (Temas 11-25)

11. **¿Qué es la Inyección de Dependencias (DI)? Menciona los 3 ciclos de vida principales. ¿Qué es Scrutor y para qué se usa? Menciona un ejemplo de cómo se configuraría un `DependenciesProvider`.**

12. **Describe los patrones Repository, Service y Factory. ¿Cómo encajan en una arquitectura Clean Architecture?**

13. **¿En qué se diferencia LINQ declarativo de imperativo? ¿Cuándo usarías `GroupBy` con `ToDictionary` frente a `GroupBy` con `Select` y `ToList`? ¿Qué es un DataFrame y cuándo lo usarías en vez de colecciones LINQ?**

14. **¿Qué es `IDisposable` y por qué es importante en la gestión de recursos? Menciona 2 bibliotecas o mecanismos para trabajar con ficheros CSV y JSON en C#. ¿Qué es `System.Text.Json`?**

15. **¿Qué es el patrón Result o ROP (Rail-Oriented Programming)? Define `DomainError`, `Maybe`, `Bind`, `Map` y `Match`. ¿Cuándo usar Result frente a Excepciones?**

16. **Explica cómo funciona async/await en C#. ¿Qué es un `Task`? ¿Qué es `CancellationToken` y por qué es importante? ¿Por qué se recomienda evitar `async void`? Menciona al menos 2 patrones de concurrencia.**

17. **¿Qué es la programación reactiva? Compara `IAsyncEnumerable` con `IObservable`. ¿Qué es Rx.NET? ¿Cuándo usar cada uno de estos enfoques?**

18. **¿Qué es un `DbContext` en EF Core? ¿Cómo funciona LINQ to Entities? Explica para qué sirven las migraciones y el método `Include`.**

19. **Compara SQL y NoSQL. ¿Qué es ACID y qué es BASE? ¿Cuándo usar PostgreSQL, MongoDB o Redis? Explica la diferencia entre Embedded y Referencias en una base de datos.**

20. **¿Qué es NUnit y para qué se usa? ¿Cómo se hace mocking con Moq? Menciona 3 tipos de aserciones con FluentAssertions. ¿Qué es TestContainers y por qué es útil? Explica el patrón AAA (Arrange-Act-Assert) en testing.**