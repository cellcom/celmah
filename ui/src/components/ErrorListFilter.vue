<template>
  <div>
    <!-- Active filter tags -->
    <div v-if="activeTags.length > 0" class="d-flex flex-wrap align-items-center p-2 bg-light border-bottom">
      <button class="btn btn-sm btn-light me-2" @click="clearAll">Clear filter</button>
      <span
        v-for="tag in activeTags"
        :key="tag"
        class="badge bg-info text-white me-1 d-inline-flex align-items-center"
        style="font-size: 0.85rem"
      >
        <a href="#" class="text-white text-decoration-none me-1" @click.prevent="editTag(tag)">{{ tag }}</a>
        <button class="btn-close btn-close-white" style="font-size: 0.5rem" @click="removeTag(tag)"></button>
      </span>
    </div>

    <!-- Filter modal -->
    <div v-if="showModal" class="modal d-block" tabindex="-1" @click.self="close">
      <div class="modal-dialog modal-lg modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">{{ editMode ? 'Edit filter' : 'Add filter' }}</h5>
            <button type="button" class="btn-close" @click="close"></button>
          </div>
          <div class="modal-body">
            <div class="row">
              <div class="col-sm">
                <label class="form-label">Property</label>
                <select class="form-select" v-model="filterProperty">
                  <option v-for="p in properties" :key="p.value" :value="p.value">{{ p.text }}</option>
                </select>
              </div>
              <div class="col-sm">
                <label class="form-label">Condition</label>
                <select class="form-select" v-model="filterCondition">
                  <option v-for="c in conditions" :key="c.value" :value="c.value">{{ c.text }}</option>
                </select>
              </div>
              <div class="col-sm">
                <label class="form-label">Value</label>
                <input
                  v-if="!isDate"
                  class="form-control"
                  v-model="textValue"
                />
                <input
                  v-if="isDate"
                  class="form-control"
                  type="date"
                  v-model="dateValue"
                />
                <input
                  v-if="isDateTime"
                  class="form-control mt-2"
                  type="time"
                  step="1"
                  v-model="timeValue"
                />
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" @click="close">Close</button>
            <button class="btn btn-info text-white" @click="submit">{{ editMode ? 'Save' : 'Add' }}</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useErrorStore } from '@/store'

const emit = defineEmits(['close'])
const store = useErrorStore()

const showModal = ref(true)
const filterProperty = ref('message')
const filterCondition = ref('=')
const textValue = ref('')
const dateValue = ref(null)
const timeValue = ref(null)
const editMode = ref(false)
const editingTag = ref(null)

const properties = [
  { value: 'application', text: 'Application' },
  { value: 'body', text: 'Body' },
  { value: 'client', text: 'Client IP' },
  { value: 'date-time', text: 'Date/Time' },
  { value: 'details', text: 'Details' },
  { value: 'host', text: 'Host' },
  { value: 'message', text: 'Message' },
  { value: 'method', text: 'Method' },
  { value: 'source', text: 'Source' },
  { value: 'status-code', text: 'Status Code' },
  { value: 'type', text: 'Type' },
  { value: 'url', text: 'Url' },
  { value: 'user', text: 'User' },
]

const conditions = [
  { value: '=', text: 'Equals' },
  { value: '!=', text: 'Not Equals' },
  { value: '~', text: 'Contains' },
  { value: '!~', text: 'Does Not Contain' },
]

const activeTags = computed(() => store.filterTags)

const isDate = computed(() => filterProperty.value === 'date-time')
const isDateTime = computed(() => isDate.value && (filterCondition.value === '=' || filterCondition.value === '!='))

watch(isDate, (v) => {
  if (v && !dateValue.value) {
    const d = new Date()
    dateValue.value = d.toISOString().substring(0, 10)
  }
})
watch(isDateTime, (v) => {
  if (v && !timeValue.value) {
    timeValue.value = '00:00:00'
  }
})

function close() {
  emit('close')
}

function clearAll() {
  store.clearFilterTags()
}

function removeTag(tag) {
  store.removeFilterTag(tag)
}

function toTag(property, condition, text, date, time) {
  let tag = property + ' ' + condition + ' '
  if (date) {
    tag += date
    if (time) tag += ' ' + time
  } else {
    tag += text
  }
  return tag.trim()
}

function submit() {
  const tag = toTag(
    filterProperty.value,
    filterCondition.value,
    !isDate.value ? textValue.value : null,
    isDate.value ? dateValue.value : null,
    isDateTime.value ? timeValue.value : null,
  )
  if (editingTag.value) {
    store.removeFilterTag(editingTag.value)
  }
  store.addFilterTag(tag)
  close()
}

function editTag(tag) {
  const match = tag.match(/([^\s]*)\s+([^\s]*)\s+(.*)/)
  if (!match) return
  filterProperty.value = match[1]
  filterCondition.value = match[2]
  const val = match[3]
  if (isDate.value) {
    dateValue.value = val
    if (isDateTime.value) {
      timeValue.value = val.substring(11, 19)
    }
  } else {
    textValue.value = val
  }
  editMode.value = true
  editingTag.value = tag
  showModal.value = true
}

// Expose addFilterTag for programmatic use from child components
function addFilterTag(tag) {
  store.addFilterTag(tag)
}

defineExpose({ addFilterTag })
</script>
