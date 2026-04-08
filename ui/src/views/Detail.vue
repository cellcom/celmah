<template>
  <div>
    <ErrorDetail :item="item" :id="id" />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { api, getCelmahRoot } from '@/api'
import { showToast } from '@/components/toast-service'
import ErrorDetail from '@/components/ErrorDetail.vue'

const props = defineProps(['id'])
const item = ref({})

onMounted(() => {
  api.get(`${getCelmahRoot()}/api/error?id=${props.id}`)
    .then(response => { item.value = response.data.error })
    .catch(error => {
      console.log(error)
      showToast('Data loading error.', 'danger')
    })
})
</script>
