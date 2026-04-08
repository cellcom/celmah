<template>
  <div class="e-view">
    <div class="e-main-content">
      <ErrorsList v-model:selected="selected" @select="onSelect" />
      <ErrorDetail
        v-if="!collapsed && selected && selected.error"
        :item="selected.error"
        :log="selected.log"
        :id="selected.id"
        @back="collapsed = !collapsed"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import ErrorDetail from '@/components/ErrorDetail.vue'
import ErrorsList from '@/components/ErrorsList.vue'

const selected = ref(null)
const collapsed = ref(false)

function onSelect(item) {
  collapsed.value = window.innerWidth <= 1024 ? !collapsed.value : false
}
</script>

<style lang="scss">
@use '../styles/variables' as *;

.e-view {
  display: flex;
  flex-direction: column;

  .e-main-content {
    flex-grow: 1;
    display: flex;
    flex-direction: row;
  }
}
@media screen and (max-width: 1024px) {
  .e-view {
    .e-main-content {
      display: block;
      .e-detail {
        position: absolute;
        width: 100%;
        height: 100%;
        top: 0;
      }
    }
  }
}
</style>
