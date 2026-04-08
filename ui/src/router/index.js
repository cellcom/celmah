import { createRouter, createWebHistory } from 'vue-router'
import List from '@/views/List.vue'
import About from '@/views/About.vue'
import Detail from '@/views/Detail.vue'

const routes = [
  { path: '/', name: 'Root', redirect: '/errors' },
  { path: '/errors', name: 'Errors', component: List },
  { path: '/about', name: 'About', component: About },
  { path: '/detail/:id', name: 'Detail', component: Detail, props: true },
  { path: '/:pathMatch(.*)*', redirect: { name: 'Errors' } },
]

function getBase() {
  const meta = document.querySelector('meta[name="celmah-root"]')
  return meta ? meta.content : '/celmah'
}

const router = createRouter({
  history: createWebHistory(getBase()),
  routes,
  scrollBehavior() {
    return { top: 0 }
  },
})

export default router
