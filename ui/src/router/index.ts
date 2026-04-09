import { createRouter, createWebHistory } from 'vue-router'
import List from '@/views/List.vue'
import Detail from '@/views/Detail.vue'
import { getRouterBase } from '@/api'

const routes = [
  { path: '/', name: 'Root', redirect: '/errors' },
  { path: '/errors', name: 'Errors', component: List },
  { path: '/detail/:id', name: 'Detail', component: Detail, props: true },
  { path: '/:pathMatch(.*)*', redirect: { name: 'Errors' } },
]

const router = createRouter({
  history: createWebHistory(getRouterBase()),
  routes,
  scrollBehavior() {
    return { top: 0 }
  },
})

export default router
