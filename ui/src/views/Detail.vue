<template>
  <div>
    <ErrorDetail :item="item" :id="id" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api, getCelmahRoot } from '@/api'
import { showToast } from '@/components/toast-service'
import ErrorDetail from '@/components/ErrorDetail.vue'
import type { ErrorDetail as ErrorDetailType } from '@/types'

const props = defineProps<{ id: string }>()
const item = ref<ErrorDetailType>({} as ErrorDetailType)

onMounted(() => {
  api.get(`${getCelmahRoot()}/api/error?id=${props.id}`)
    .then(response => { item.value = response.data.error })
    .catch(error => {
      console.log(error)
      showToast('Data loading error.', 'danger')
    })
})
</script>
