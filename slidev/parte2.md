---
theme: default
title: 'UD01 · Parte 2 · C# avanzado para desarrollo servidor'
info: |
  C# avanzado para desarrollo servidor
  Desarrollo Web en Entorno Servidor · 2DAW · Curso 2026-2027 · IES Luis Vives
  José Luis González Sánchez
author: José Luis González Sánchez
transition: slide-left
colorScheme: auto
lineNumbers: false
layout: cover
class: ud-cover-bg
---

<div class="flex items-center gap-4 w-full">
  <img src="/logo.webp" alt="Logo" class="h-11 rounded-md shadow" />
  <span class="ud-pill">Desarrollo Web en Entorno Servidor · 2DAW · 2026-2027</span>
</div>

# Desarrollo web en servidor

### Parte 2: C# avanzado

<div class="mt-10 flex items-end justify-between w-full">
  <div class="text-left">
    <b class="text-lg">José Luis González Sánchez</b><br/>
    <span class="opacity-70">IES Luis Vives · Leganés</span>
  </div>
  <PacMan :size="54" label="Preparando la aplicación..." />
</div>

---

## 🎯 Resultado de aprendizaje: RA 1

<RaaCcee
  code="RA 1"
  title="Selecciona las arquitecturas y tecnologías de programación web en entorno servidor, analizando sus capacidades y características propias."
  :letters="['a', 'b', 'c', 'd', 'e', 'f', 'g']"
  :ccee="[
    'Se han caracterizado y diferenciado los modelos de ejecución de código en el servidor y en el cliente web.',
    'Se han reconocido las ventajas que proporciona la generación dinámica de páginas.',
    'Se han identificado los mecanismos de ejecución de código en los servidores web.',
    'Se han reconocido las funcionalidades que aportan los servidores de aplicaciones y su integración con los servidores web.',
    'Se han identificado y caracterizado los principales lenguajes y tecnologías relacionados con la programación web en entorno servidor.',
    'Se han verificado los mecanismos de integración de los lenguajes de marcas con los lenguajes de programación en entorno servidor.',
    'Se han reconocido y evaluado las herramientas y frameworks de programación en entorno servidor.',
  ]"
/>

---

## 📚 Parte 2: C# avanzado (1/2)

<div class="grid grid-cols-2 gap-3">

<TopicCard :num="11" title="Inyección de dependencias" desc="Abstracciones y ciclos de vida" />
<TopicCard :num="12" title="Patrones y arquitecturas" desc="Repository, Service y Factory" />
<TopicCard :num="13" title="LINQ y DataFrames" desc="Consultas y datos" />
<TopicCard :num="14" title="Ficheros y formatos" desc="Streams y serialización" />
<TopicCard :num="15" title="Result y excepciones" desc="Modelar los errores" />
<TopicCard :num="16" title="Concurrencia y asincronía" desc="Tareas y cancelación" />
<TopicCard :num="17" title="Programación reactiva" desc="Flujos de datos" />

</div>

---

## 📚 Parte 2: C# avanzado (2/2)

<div class="grid grid-cols-2 gap-3">

<TopicCard :num="18" title="Consumo de APIs con Refit" desc="Clientes y resiliencia" />
<TopicCard :num="19" title="Configuration y logging" desc="Opciones y registros" />
<TopicCard :num="20" title="Entity Framework Core" desc="Persistencia y migraciones" />
<TopicCard :num="21" title="SQL y NoSQL" desc="ACID, BASE y caché" />
<TopicCard :num="22" title="Testing avanzado" desc="Pruebas y depuración" />
<TopicCard :num="23" title="Docker" desc="Contenedores y Compose" />
<TopicCard :num="24" title="Seguridad" desc="Autenticación y protección" />

</div>
