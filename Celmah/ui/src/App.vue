<template>
  <div id="app">
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <router-link class="navbar-brand" :to="{ name: 'Errors' }">Celmah</router-link>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#nav-collapse">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div id="nav-collapse" class="collapse navbar-collapse">
        <ul class="navbar-nav me-auto">
          <li class="nav-item">
            <router-link class="nav-link" :to="{ name: 'Errors' }">Errors</router-link>
          </li>
          <li class="nav-item">
            <a class="nav-link" target="_blank" :href="celmahRoot + '/rss'">RSS</a>
          </li>
          <li class="nav-item">
            <a class="nav-link" target="_blank" :href="celmahRoot + '/digestrss'">RSS Digest</a>
          </li>
          <li class="nav-item">
            <a class="nav-link" target="_blank" :href="celmahRoot + '/download'">Download</a>
          </li>
          <li class="nav-item">
            <a class="nav-link d-flex align-items-center" target="_blank" href="https://github.com/cellcom/celmah">
              <GithubIcon :size="18" />
            </a>
          </li>
        </ul>
        <div class="d-flex align-items-center gap-2">
          <template v-if="$route.name === 'Errors'">
            <button class="btn btn-outline-light btn-sm d-flex align-items-center" @click="showFilterModal = true" title="Add Filter">
              <FilterIcon :size="14" />
            </button>
            <div class="input-group input-group-sm">
              <span class="input-group-text"><SearchIcon :size="14" /></span>
              <input
                class="form-control"
                placeholder="Search"
                v-model="searchText"
                @keydown.enter.prevent="search"
              />
            </div>
          </template>
          <button class="btn btn-outline-light btn-sm ms-1 d-flex align-items-center" @click="toggleTheme" :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'">
            <SunIcon v-if="isDark" :size="15" />
            <MoonIcon v-else :size="15" />
          </button>
        </div>
      </div>
    </nav>
    <ErrorListFilter v-if="showFilterModal" @close="showFilterModal = false" />
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { FilterIcon, SearchIcon, GithubIcon, SunIcon, MoonIcon } from 'lucide-vue-next'
import { useErrorStore } from './store'
import ErrorListFilter from '@/components/ErrorListFilter.vue'

const store = useErrorStore()
const searchText = ref('')
const showFilterModal = ref(false)
const isDark = ref(false)

const celmahRoot = computed(() => (window as any).$celmah_root as string)

function search() {
  store.setSearchText(searchText.value)
}

function toggleTheme() {
  isDark.value = !isDark.value
  document.documentElement.setAttribute('data-theme', isDark.value ? 'dark' : 'light')
  localStorage.setItem('celmah-theme', isDark.value ? 'dark' : 'light')
}

onMounted(() => {
  const saved = localStorage.getItem('celmah-theme')
  if (saved === 'dark' || (!saved && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
    isDark.value = true
    document.documentElement.setAttribute('data-theme', 'dark')
  }
})
</script>

<style lang="scss">
@use './styles/variables' as *;

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
}

html, body {
  overflow-y: hidden;
}

.navbar-brand {
  font-weight: 600;
}
</style>
