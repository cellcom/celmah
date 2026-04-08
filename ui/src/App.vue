<template>
  <div id="app">
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <a class="navbar-brand" href="#">Celmah</a>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#nav-collapse">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div id="nav-collapse" class="collapse navbar-collapse">
        <ul class="navbar-nav me-auto">
          <li class="nav-item">
            <router-link class="nav-link" :to="{ name: 'Errors' }">Errors</router-link>
          </li>
          <li class="nav-item">
            <a class="nav-link" target="_blank" :href="celmahRoot + '/rss'">RSS Feeds</a>
          </li>
          <li class="nav-item">
            <a class="nav-link" target="_blank" :href="celmahRoot + '/digestrss'">RSS Digest</a>
          </li>
          <li class="nav-item">
            <a class="nav-link" target="_blank" :href="celmahRoot + '/download'">Download Log</a>
          </li>
          <li class="nav-item">
            <a class="nav-link" target="_blank" href="https://github.com/cellcom/celmah">Help</a>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" :to="{ name: 'About' }">About</router-link>
          </li>
        </ul>
        <div v-if="$route.name === 'Errors'" class="d-flex align-items-center">
          <button class="btn btn-light btn-sm me-2" @click="showFilterModal = true">
            <FilterIcon :size="14" class="me-1" />
            <span style="font-size: 0.9rem">Add Filter</span>
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
        </div>
      </div>
    </nav>
    <ErrorListFilter v-if="showFilterModal" @close="showFilterModal = false" />
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { FilterIcon, SearchIcon } from 'lucide-vue-next'
import { useErrorStore } from './store'
import ErrorListFilter from '@/components/ErrorListFilter.vue'

const store = useErrorStore()
const searchText = ref('')
const showFilterModal = ref(false)

const celmahRoot = computed(() => (window as any).$celmah_root as string)

function search() {
  store.setSearchText(searchText.value)
}
</script>

<style lang="scss">
@use './styles/variables' as *;

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
}

html, body {
  overflow-y: hidden;
}

// Expose ErrorListFilter ref for programmatic access from child components
// via the global property pattern
</style>
