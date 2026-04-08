<template>
  <pre class="hljs-pre"><code ref="codeEl" :class="'language-' + lang">{{ code }}</code></pre>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { hljs } from '@/utils'

const props = withDefaults(defineProps<{
  lang?: string
  code?: string
}>(), {
  lang: 'json',
  code: '',
})

const codeEl = ref<HTMLElement | null>(null)

function highlight() {
  if (codeEl.value) {
    const result = hljs.highlight(props.code, { language: props.lang })
    codeEl.value.innerHTML = result.value
  }
}

onMounted(highlight)
watch(() => props.code, highlight)
</script>

<style scoped>
.hljs-pre {
  padding: 15px;
  font-size: 14px;
  background: #f0f0f0;
  font-family: SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace;
  overflow-wrap: anywhere;
  white-space: pre-wrap;
  margin: 0;
}
</style>
