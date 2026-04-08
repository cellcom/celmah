<template>
  <div class="toast-container position-fixed bottom-0 end-0 p-3" style="z-index: 9999">
    <div
      v-for="toast in toasts"
      :key="toast.id"
      class="toast show"
      :class="'text-bg-' + toast.variant"
      role="alert"
    >
      <div class="d-flex">
        <div class="toast-body">{{ toast.message }}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" @click="remove(toast.id)"></button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { toasts, remove } from './toast-service'
</script>
