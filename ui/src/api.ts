import axios from 'axios'

axios.defaults.headers['x-requested-with'] = 'XMLHttpRequest'

export const api = axios.create()

/** API base path — always /celmah (proxied in dev, real in prod) */
export function getCelmahRoot(): string {
  return '/celmah'
}

/** Vue Router base — / in dev (Vite root), /celmah in prod (embedded) */
export function getRouterBase(): string {
  if (import.meta.env.DEV) return '/'
  const meta = document.querySelector('meta[name="celmah-root"]') as HTMLMetaElement | null
  return meta ? meta.content : '/celmah'
}
