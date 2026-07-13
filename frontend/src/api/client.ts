import axios from 'axios'
import type { AxiosError } from 'axios'

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Attach the JWT token to every request if present.
client.interceptors.request.use((config) => {
  const token = localStorage.getItem('em_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// On 401, clear the session so the app returns to the login screen.
client.interceptors.response.use(
  (res) => res,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('em_token')
      localStorage.removeItem('em_user')
      if (!window.location.pathname.startsWith('/login')) {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export default client
