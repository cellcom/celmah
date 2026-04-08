<template>
  <div class="e-list">
    <div
      class="e-list-content"
      ref="scrollEl"
      :class="{ loading: loading }"
    >
      <ErrorListItem
        v-for="(item, index) in items"
        :key="item.id"
        :class="{ gray: index % 2 === 0 }"
        :item="item.error"
        :id="item.id"
        :is-selected="selected && selected.id === item.id"
        @select="selectItem(item)"
      />
    </div>
    <div class="total-count">
      Loaded <span>{{ items.length }}</span> of
      <span>{{ totalCount }}</span> errors
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { useErrorStore } from '@/store'
import { api, getCelmahRoot } from '@/api'
import { showToast } from './toast-service'
import ErrorListItem from './ErrorListItem.vue'

const emit = defineEmits(['select'])

const store = useErrorStore()
const scrollEl = ref(null)
const items = ref([])
const totalCount = ref(0)
const loading = ref(false)
const errorIndex = ref(0)
const loaded = ref(false)
const loadTimerId = ref(null)
const loadNewTimerStarted = ref(false)
const filtersHash = ref('')
const selected = defineModel('selected')

watch(() => store.searchText, () => loadErrors())
watch(() => store.filterTags, () => {
  const newHash = store.filtersHash
  if (newHash !== filtersHash.value) {
    filtersHash.value = newHash
    loadErrors()
  }
}, { deep: true })

onMounted(() => {
  loadErrors()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  if (loadTimerId.value) clearTimeout(loadTimerId.value)
})

function handleResize() {
  if (!scrollEl.value) return
  const height = window.innerHeight - scrollEl.value.offsetTop - 30
  scrollEl.value.style.height = height + 'px'
}

function setupScroll() {
  const el = scrollEl.value
  if (!el) return
  el.onscroll = () => {
    if (loading.value || loaded.value) return
    const bottomOfWindow = el.scrollTop + el.clientHeight >= el.scrollHeight - 2
    if (bottomOfWindow) {
      loadMore()
    }
  }
}

function loadMore() {
  loading.value = true
  api.get(`${getCelmahRoot()}/api/errors?i=${errorIndex.value}&s=50`)
    .then(response => {
      if (response.data && response.data.errors.length > 0) {
        errorIndex.value += response.data.errors.length
      } else {
        loaded.value = true
      }
      loading.value = false
      items.value = items.value.concat(response.data.errors)
      totalCount.value = response.data.totalCount
    })
    .catch(error => {
      loading.value = false
      console.log(error)
      showToast('Data loading error.', 'danger')
    })
}

function loadErrors() {
  if (loadTimerId.value != null) {
    clearTimeout(loadTimerId.value)
    loadTimerId.value = null
  }
  const filterTags = store.filterTags
  const searchText = store.searchText

  api.post(`${getCelmahRoot()}/api/errors?p=0&s=50&q=${encodeURIComponent(searchText)}`, filterTags)
    .then(response => {
      items.value = response.data.errors
      errorIndex.value = response.data.errors.length
      totalCount.value = response.data.totalCount
      if (items.value.length > 0) {
        selected.value = items.value[0]
      }
    })
    .catch(error => {
      console.log(error)
      showToast('Data loading error.', 'danger')
    })
    .finally(() => {
      handleResize()
      nextTick(setupScroll)
      if (!loadNewTimerStarted.value) {
        loadNewTimerStarted.value = true
        loadTimerId.value = setTimeout(() => loadNewErrors(), 10000)
      }
    })
}

function loadNewErrors() {
  const filterTags = store.filterTags
  const searchText = store.searchText
  const id = items.value.length > 0 ? items.value[0].id : ''

  api.post(`${getCelmahRoot()}/api/new-errors?id=${id}&q=${encodeURIComponent(searchText)}`, filterTags)
    .then(response => {
      if (response.data?.errors?.length) {
        const size = Math.min(100, response.data.totalCount)
        items.value = response.data.errors.concat(items.value).slice(0, size)
        totalCount.value = response.data.totalCount
        errorIndex.value += response.data.errors.length
        showToast(`${response.data.errors.length} new error(s) loaded.`, 'warning')
      }
    })
    .catch(error => {
      console.log(error)
      showToast('Data loading error.', 'danger')
    })
    .finally(() => {
      loadTimerId.value = setTimeout(loadNewErrors, 10000)
    })
}

function selectItem(item) {
  selected.value = item
  emit('select', item)
}
</script>

<style lang="scss" scoped>
@use '../styles/variables' as *;
.e-list {
  width: 40%;
  max-width: 500px;
  min-width: 350px;
  flex-shrink: 0;
  border-right: 1px solid $border-main-color;

  .total-count {
    margin: 4px;
    text-align: start;
    font-size: 13px;
    span { font-weight: 600; }
  }
  .e-list-content {
    overflow-y: scroll;
    overflow-x: hidden;
  }
}
@media screen and (max-width: 1024px) {
  .e-list {
    width: 100%;
    max-width: none;
    min-width: 200px;
  }
}
</style>
