<script setup lang="ts">
/**
 * Lista de objetivos de aprendizaje de cada punto del tema.
 */
defineProps<{
  items: string[]
}>()
</script>

<template>
  <div class="ud-objetivos">
    <div class="ud-objetivos-head">🎯 Objetivos de aprendizaje</div>
    <ul class="ud-objetivos-list">
      <li v-for="(item, i) in items" :key="i" v-click>
        {{ item }}
      </li>
    </ul>
  </div>
</template>

<style scoped>
.ud-objetivos {
  text-align: left;
}

.ud-objetivos-head {
  font-weight: 700;
  color: var(--ud-text);
  font-size: 1.05em;
  margin-bottom: 0.5em;
}

.ud-objetivos-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.ud-objetivos-list li {
  position: relative;
  padding-left: 1.6em;
  margin: 0.45em 0;
  color: var(--ud-text-mute);
  font-size: 0.95em;
  line-height: 1.5;
}

.ud-objetivos-list li::before {
  content: '▹';
  position: absolute;
  left: 0.3em;
  color: var(--ud-accent);
  font-weight: 700;
}
</style>
