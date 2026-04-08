import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import ToastContainer from './components/ToastContainer.vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'highlight.js/styles/default.css'
import './styles/main.scss'

const app = createApp(App)
app.use(createPinia())
app.use(router)

// Mount ToastContainer as a sibling of App
const root = document.getElementById('app')
app.mount(root)

// Create toast container outside app
const toastDiv = document.createElement('div')
toastDiv.id = 'toast-root'
document.body.appendChild(toastDiv)

const toastApp = createApp(ToastContainer)
toastApp.mount(toastDiv)
