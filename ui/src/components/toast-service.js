import { reactive } from 'vue'

let nextId = 0
export const toasts = reactive([])

export function showToast(message, variant = 'success', duration = 2000) {
  const id = nextId++
  toasts.push({ id, message, variant })
  if (duration > 0) {
    setTimeout(() => remove(id), duration)
  }
}

export function remove(id) {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx !== -1) toasts.splice(idx, 1)
}
