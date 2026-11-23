<script setup lang="ts">
/**
 * PacMan (comecocos) animado con los puntos que come.
 *
 * Réplica del LoadingPage.vue real de joseluisgs.dev:
 *  - Mismo color: blurple #7289da (el acento de la web), nunca amarillo.
 *  - Cuerpo de dos mitades que chascan (la boca se abre ~90°).
 *  - Los puntos viajan de derecha a izquierda y se colan por la boca.
 *
 * La pista de los puntos se dimensiona a partir de `size`, así el conjunto
 * escala igual en portada y en el cierre.
 */
withDefaults(
  defineProps<{
    /** Tamaño del comecocos en píxeles */
    size?: number
    /** Número de puntos que come */
    dots?: number
    /** Texto que aparece debajo (opcional) */
    label?: string
  }>(),
  { size: 72, dots: 3, label: '' },
)
</script>

<template>
  <div class="pac-wrap" :style="{ '--pac-size': `${size}px` }">
    <div class="pac-scene" aria-hidden="true">
      <div class="pac">
        <span class="pac-half pac-half--top" />
        <span class="pac-half pac-half--bottom" />
      </div>

      <span class="pac-track">
        <span
          v-for="i in dots"
          :key="i"
          class="pac-dot"
          :style="{ '--dot-delay': `${((i - 1) / dots) * 1.4}s` }"
        />
      </span>
    </div>

    <span v-if="label" class="pac-label">{{ label }}</span>
  </div>
</template>

<style scoped>
.pac-wrap {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: 0.7em;
}

.pac-scene {
  display: flex;
  align-items: center;
  gap: 0.25em;
  /* La pista mide lo mismo que el cuerpo: los puntos entran en la boca */
  --track: calc(var(--pac-size) * 0.8);
  --dot-size: calc(var(--pac-size) * 0.17);
}

.pac {
  position: relative;
  z-index: 2;
  display: inline-block;
  flex: none;
  width: var(--pac-size);
  height: var(--pac-size);
}

.pac-half {
  position: absolute;
  left: 0;
  width: 100%;
  height: 50%;
  background: var(--ud-accent);
  backface-visibility: hidden;
}

.pac-half--top {
  top: 0;
  border-radius: 999px 999px 0 0;
  transform-origin: bottom center;
  animation: pac-chomp-top 0.5s ease-in-out infinite;
}

.pac-half--bottom {
  top: 50%;
  border-radius: 0 0 999px 999px;
  transform-origin: top center;
  animation: pac-chomp-bottom 0.5s ease-in-out infinite;
}

/* --- Pista y puntos (bolitas) que come --- */
.pac-track {
  position: relative;
  z-index: 1;
  flex: none;
  width: var(--track);
  height: var(--dot-size);
}

.pac-dot {
  position: absolute;
  top: 50%;
  right: 0;
  width: var(--dot-size);
  height: var(--dot-size);
  margin-top: calc(var(--dot-size) / -2);
  border-radius: 50%;
  background: var(--ud-accent);
  animation: pac-eat-dot 1.4s linear infinite;
  animation-delay: var(--dot-delay);
}

/* La boca abre ~90°: mitad superior sube, mitad inferior baja */
@keyframes pac-chomp-top {
  0%,
  100% {
    transform: rotate(0deg);
  }
  50% {
    transform: rotate(-45deg);
  }
}

@keyframes pac-chomp-bottom {
  0%,
  100% {
    transform: rotate(0deg);
  }
  50% {
    transform: rotate(45deg);
  }
}

/* El punto aparece a la derecha, viaja hacia la boca y se esconde detrás */
@keyframes pac-eat-dot {
  0% {
    transform: translateX(0);
    opacity: 0;
  }
  6% {
    transform: translateX(0);
    opacity: 1;
  }
  78% {
    transform: translateX(calc(var(--track) * -1.3));
    opacity: 1;
  }
  92%,
  100% {
    transform: translateX(calc(var(--track) * -1.3));
    opacity: 0;
  }
}

.pac-label {
  font-size: 0.8em;
  color: var(--ud-text-subtle);
  letter-spacing: 0.04em;
}
</style>
