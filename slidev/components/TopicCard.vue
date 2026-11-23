<script setup lang="ts">
/**
 * Índice de los puntos del tema: tarjeta numerada para el slide de índice.
 */
defineProps<{
  num: string | number
  title: string
  desc?: string
}>()
</script>

<template>
  <div class="ud-topic">
    <span class="ud-topic-num">{{ num }}</span>
    <div class="ud-topic-body">
      <div class="ud-topic-title">{{ title }}</div>
      <div v-if="desc" class="ud-topic-desc">{{ desc }}</div>
    </div>
  </div>
</template>

<style scoped>
.ud-topic {
  display: flex;
  align-items: center;
  gap: 0.7em;
  background: var(--ud-bg-elv);
  border: 1px solid var(--ud-divider);
  border-radius: var(--ud-radius);
  padding: 0.5em 0.8em;
  box-shadow: var(--ud-shadow);
  text-align: left;
  height: 100%;
  transition: transform 0.2s ease, border-color 0.2s ease;
}

.ud-topic:hover {
  transform: translateY(-2px);
  border-color: var(--ud-accent);
}

.ud-topic-num {
  flex: none;
  display: grid;
  place-items: center;
  width: 2em;
  height: 2em;
  border-radius: 10px;
  background: var(--ud-accent-soft);
  border: 1px solid var(--ud-accent);
  color: var(--ud-accent-strong);
  font-weight: 700;
  font-size: 0.9em;
}

html.dark .ud-topic-num {
  color: #a3b1ee;
}

.ud-topic-title {
  font-weight: 700;
  font-size: 0.9em;
  color: var(--ud-text);
  line-height: 1.25;
}

.ud-topic-desc {
  font-size: 0.72em;
  color: var(--ud-text-subtle);
  line-height: 1.3;
}
</style>
