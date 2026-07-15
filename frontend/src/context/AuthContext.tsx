import { createContext, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import client from '../api/client'
import type { AuthUser, AuthResponse } from '../types'

interface AuthContextType {
  user: AuthUser | null
  ready: boolean
  login: (userNameOrEmail: string, password: string) => Promise<void>
  register: (userName: string, email: string, password: string) => Promise<void>
  resetPassword: (userName: string, email: string, newPassword: string) => Promise<string>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const raw = localStorage.getItem('em_user')
    return raw ? (JSON.parse(raw) as AuthUser) : null
  })
  const [ready, setReady] = useState(false)

  useEffect(() => {
    setReady(true)
  }, [])

  const persist = (data: AuthResponse) => {
    localStorage.setItem('em_token', data.token)
    const u: AuthUser = { userName: data.userName, email: data.email }
    localStorage.setItem('em_user', JSON.stringify(u))
    setUser(u)
  }

  const login = async (userNameOrEmail: string, password: string) => {
    const { data } = await client.post<AuthResponse>('/auth/login', { userNameOrEmail, password })
    persist(data)
  }

  const register = async (userName: string, email: string, password: string) => {
    const { data } = await client.post<AuthResponse>('/auth/register', { userName, email, password })
    persist(data)
  }

  const resetPassword = async (userName: string, email: string, newPassword: string) => {
    const { data } = await client.post<{ message: string }>('/auth/reset-password', { userName, email, newPassword })
    return data.message
  }

  const logout = () => {
    localStorage.removeItem('em_token')
    localStorage.removeItem('em_user')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, ready, login, register, resetPassword, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextType {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within an AuthProvider')
  return ctx
}
