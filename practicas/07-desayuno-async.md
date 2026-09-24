# Práctica 7: Desayuno Asíncrono

- [Práctica 7: Desayuno Asíncrono](#práctica-7-desayuno-asíncrono)
  - [Objetivo](#objetivo)
  - [Descripción](#descripción)
  - [Las 7 Acciones del Desayuno](#las-7-acciones-del-desayuno)
  - [Restricción](#restricción)
  - [Tareas a Realizar](#tareas-a-realizar)
  - [Entrega](#entrega)

---

## Objetivo

Comprender la importancia del **diseño** en la preparación de un desayuno asíncrono y aprender a optimizar tiempos de ejecución.

---

## Descripción

Un desarrollador quiere automatizar la preparación de su desayuno. Cada acción tiene un tiempo de ejecución conocido. El problema es que el usuario tiene **poco tiempo por la mañana** y no puede esperar eternamente.

Tu trabajo es implementar diferentes enfoques de ejecución y descubrir cuál es el adecuado, **analizando por qué unos funcionan y otros no**.

> *"No se trata de correr más rápido, sino de saber qué carreras correr en paralelo."*

---

## Las 7 Acciones del Desayuno

| # | Acción | Tiempo | Descripción |
|---|--------|--------|-------------|
| 1 | Hacer café | 200ms | Encender la cafetera y esperar |
| 2 | Calentar sartén | 200ms | Poner el fuego y esperar a que esté caliente |
| 3 | Freír huevos | 300ms | Necesita la sartén caliente (acción 2) |
| 4 | Freír bacon | 300ms | Necesita la sartén caliente (acción 2) |
| 5 | Tostar pan | 200ms | Meter el pan en la tostadora |
| 6 | Untar mantequilla | 100ms | Necesita el pan tostado (acción 5) |
| 7 | Hacer zumo | 200ms | Exprimir las naranjas |

---

## Restricción

El usuario tiene un **tiempo límite de 500ms**. Si el desayuno no está listo a tiempo:

> ☕ **"¡El café se ha enfriado! Los huevos y tostadas con café frío no tienen gracia..."**

---

## Tareas a Realizar

1. Ejecución **secuencial**
2. Ejecución con **`async/await`**
3. Ejecución con el **mejor rendimiento posible**
4. Las tres soluciones anteriores con **tiempo límite de 500 ms** (para que el café no se enfríe)

Compara los tiempos de ejecución de las 5 soluciones y reflexiona sobre las diferencias.

---

## Entrega

### 1. Repositorio GitHub

Sube tu proyecto a GitHub. El `README.md` debe incluir:
- Tabla de tiempos de cada enfoque
- Respuestas a estas preguntas:
  1. ¿Qué diferencias has observado entre las 5 soluciones?
  2. ¿Qué acciones se pueden ejecutar a la vez y cuáles no? ¿Por qué?
  3. ¿Qué ha pasado con cada solución cuando aplicas el tiempo límite?
  4. ¿El enfoque con mejor rendimiento es también el más seguro? ¿Por qué?
  5. ¿Merece la pena complicarse con paralelismo o con mecanismos de control de tiempo? Justifica tu respuesta.

### 2. Aula Virtual

Entrega en **Aula Virtual**:
- Enlace al repositorio GitHub
- Fichero PDF con las respuestas a las preguntas

### Plazo

Fecha límite de entrega: **La fijada en el Aula Virtual**

> ⚠️ **Importante:** No se aceptan entregas fuera de plazo. Asegúrate de que el repositorio es **público** o que compartes el enlace con el profesor.
