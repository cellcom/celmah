import { reactive } from 'vue'
import type { Toast } from '@/types'

let nextId = 0
export const toasts: Toast[] = reactive([])

export function showToast(message: string, variant = 'success', duration = 2000): void {
  const id = nextId++
  toasts.push({ id, message, variant })
  if (duration > 0) {
    setTimeout(() => remove(id), duration)
  }
}

export function remove(id: number): void {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx !== -1) toasts.splice(idx, 1)
}
