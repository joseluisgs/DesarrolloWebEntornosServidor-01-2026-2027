# Ejemplo 14: Programación Reactiva con Rx.NET — Flujo Caliente

Sistema de notificaciones con un sensor que produce eventos y dos consumidores con filtros y tiempos distintos.

## Tecnologías

- .NET 10, C# 14
- System.Reactive (Rx.NET)

## Ejecutar

```bash
dotnet run --project 14-ReactividadRxNet
```

## ¿Qué es un flujo caliente?

Un **flujo caliente** (hot observable) es un stream de datos que emite eventos **independientemente de si hay suscriptores**. Si un observador se conecta tarde, **se pierde lo que ya pasó**. Solo ve los eventos desde el momento en que se suscribe.

**Analogía:** Es como un partido de fútbol en directo. Si llegas a los 20 minutos, te perdiste los primeros 20. No puedes "rebobinar" el partido. Eso es un flujo caliente.

**Flujo frío** (cold observable) sería como ver un partido en diferido: puedes empezar desde el principio aunque lo pongas una hora después.

```mermaid
flowchart LR
    subgraph SENSOR["Sensor (emisor)"]
        S["Produce eventos<br/>cada 1-5s"]
    end

    subgraph C1["Consumidor-1"]
        C1S["Se suscribe<br/>EN EL INICIO"]
        C1R["Ve: #1, #3, #4...<br/>TODOS los eventos"]
    end

    subgraph C2["Consumidor-2"]
        C2S["Se suscribe<br/>8s DESPUÉS"]
        C2R["Ve: #4, #5, #6...<br/>SOLO los que llegan después"]
    end

    S -->|"#1, #2, #3, #4..."| C1S
    S -->|"#1 ❌ #2 ❌ #3 ❌ #4 ✅"| C2S

    style SENSOR fill:#2196F3,color:#fff
    style C1 fill:#4CAF50,color:#fff
    style C2 fill:#FF9800,color:#fff
```

## Filtros de cada consumidor

| Consumidor | Muestra | Filtra |
|------------|---------|--------|
| **Consumidor-1** | CREATE 🟢, UPDATE 🟡 | ERROR 💥, DELETE 🔴 |
| **Consumidor-2** | CREATE 🟢, DELETE 🔴, ERROR 💥 | UPDATE 🟡 |

- Los errores se muestran en **rojo brillante** con `⚠️ ERROR`
- Cada consumidor cuenta sus propios mensajes (independientes)
- Cuando ambos llegan a 30, el programa termina

## Conceptos clave

- **Subject\<T\>**: Emisor que implementa `IObservable<T>` e `IObserver<T>`
- **Subscribe**: Conecta un observador al flujo
- **Where**: Filtra eventos según una condición
- **Select**: Transforma los elementos del flujo
- **Hot vs Cold**: Hot = emite sin suscriptores; Cold = empieza al suscribirse
