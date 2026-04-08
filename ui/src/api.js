import axios from 'axios'

axios.defaults.headers['x-requested-with'] = 'XMLHttpRequest'

export const api = axios.create()

export function getCelmahRoot() {
  const meta = document.querySelector('meta[name="celmah-root"]')
  return meta ? meta.content : '/celmah'
}
