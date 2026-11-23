<script setup lang="ts">
/**
 * Insignia del repositorio de la unidad con el icono de GitHub.
 * Se usa en la portada y en el cierre de la presentaciÃ³n.
 */
withDefaults(
  defineProps<{
    /** RÃ³tulo pequeÃ±o encima del badge (p. ej. "Materiales de la unidad") */
    caption?: string
    /** TamaÃ±o del badge */
    size?: 'sm' | 'lg'
  }>(),
  { caption: undefined, size: 'sm' },
)

const url = 'https://github.com/joseluisgs/DesarrolloWebEntornosServidor-01-2026-2027'
</script>

<template>
  <div class="ud-repo-wrap">
    <span v-if="caption" class="ud-repo-caption">{{ caption }}</span>
    <a
      :href="url"
      target="_blank"
      rel="noopener"
      class="ud-repo"
      :class="`ud-repo--${size}`"
    >
    <svg class="ud-repo-icon" viewBox="0 0 16 16" aria-hidden="true">
      <path
        fill="currentColor"
        d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38
           0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01
           1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95
           0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82a7.4 7.4 0 0 1 2-.27c.68 0
           1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82
           2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01
           2.2 0 .21.15.46.55.38A8.01 8.01 0 0 0 16 8c0-4.42-3.58-8-8-8Z"
      />
    </svg>
    <span class="ud-repo-label">
      <span class="ud-repo-org">joseluisgs</span>
      <span class="ud-repo-sep">/</span>
      <span class="ud-repo-name">DesarrolloWebEntornosServidor-01-2026-2027</span>
    </span>
    <span class="ud-repo-cta" aria-label="Ver repositorio">→</span>
    </a>
  </div>
</template>

<style scoped>
.ud-repo-wrap {
  display: inline-flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.35em;
}

.ud-repo-caption {
  font-size: 0.62em;
  letter-spacing: 0.14em;
  text-transform: uppercase;
  font-weight: 700;
  color: var(--ud-text-subtle);
}
.ud-repo {
  display: inline-flex;
  align-items: center;
  gap: 0.55em;
  text-decoration: none;
  border-radius: 999px;
  padding: 0.45em 0.9em;
  font-family: var(--ud-font-mono);
  font-size: 0.8em;
  line-height: 1;
  color: var(--ud-text);
  background: linear-gradient(
    135deg,
    var(--ud-bg-elv) 0%,
    var(--ud-bg-alt) 100%
  );
  border: 1px solid var(--ud-divider);
  box-shadow: var(--ud-shadow);
  transition:
    transform 0.2s ease,
    border-color 0.2s ease,
    box-shadow 0.2s ease;
}

.ud-repo:hover {
  transform: translateY(-2px);
  border-color: var(--ud-accent);
  box-shadow: 0 6px 20px rgba(114, 137, 218, 0.35);
}

.ud-repo-icon {
  flex: none;
  width: 1.15em;
  height: 1.15em;
  color: var(--ud-accent-strong);
}

html.dark .ud-repo-icon {
  color: #a3b1ee;
}

.ud-repo-label {
  display: inline-flex;
  gap: 0.35em;
  white-space: nowrap;
}

.ud-repo-org {
  color: var(--ud-text-subtle);
}

.ud-repo-sep {
  color: var(--ud-accent);
  font-weight: 700;
}

.ud-repo-name {
  font-weight: 700;
  color: var(--ud-text);
}

.ud-repo-cta {
  flex: none;
  white-space: nowrap;
  font-weight: 700;
  color: var(--ud-on-accent);
  background: var(--ud-accent);
  border-radius: 999px;
  padding: 0.3em 0.7em;
  font-size: 0.85em;
  animation: ud-repo-pulse 2.4s ease-in-out infinite;
}

@keyframes ud-repo-pulse {
  0%,
  100% {
    box-shadow: 0 0 0 0 rgba(114, 137, 218, 0.55);
  }
  50% {
    box-shadow: 0 0 0 6px rgba(114, 137, 218, 0);
  }
}

/* Variante grande para el cierre */
.ud-repo--lg {
  font-size: 1.15em;
  padding: 0.6em 1.3em;
  box-shadow: var(--ud-shadow-lg);
}

.ud-repo-wrap:has(.ud-repo--lg) {
  align-items: center;
}
</style>
