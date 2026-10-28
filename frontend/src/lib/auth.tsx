import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { api, unwrap } from './api'
import type { ApiResponse, AuthResult, UserProfile } from '../types/api'

interface AuthContextValue {
  user: UserProfile | null
  token: string | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (payload: {
    email: string
    password: string
    firstName: string
    lastName: string
  }) => Promise<void>
  logout: () => Promise<void>
  hasRole: (...roles: string[]) => boolean
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

function readStoredUser(): UserProfile | null {
  const raw = localStorage.getItem('if_user')
  if (!raw) return null
  try {
    return JSON.parse(raw) as UserProfile
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem('if_access_token'))
  const [user, setUser] = useState<UserProfile | null>(readStoredUser)

  const persist = (result: AuthResult) => {
    localStorage.setItem('if_access_token', result.accessToken)
    localStorage.setItem('if_refresh_token', result.refreshToken)
    localStorage.setItem('if_user', JSON.stringify(result.user))
    setToken(result.accessToken)
    setUser(result.user)
  }

  const clear = () => {
    localStorage.removeItem('if_access_token')
    localStorage.removeItem('if_refresh_token')
    localStorage.removeItem('if_user')
    setToken(null)
    setUser(null)
  }

  const login = useCallback(async (email: string, password: string) => {
    const { data } = await api.post<ApiResponse<AuthResult>>('/api/auth/login', { email, password })
    persist(unwrap(data))
  }, [])

  const register = useCallback(async (payload: {
    email: string
    password: string
    firstName: string
    lastName: string
  }) => {
    const { data } = await api.post<ApiResponse<AuthResult>>('/api/auth/register', payload)
    persist(unwrap(data))
  }, [])

  const logout = useCallback(async () => {
    const refreshToken = localStorage.getItem('if_refresh_token')
    try {
      if (refreshToken) {
        await api.post('/api/auth/logout', { refreshToken })
      }
    } finally {
      clear()
    }
  }, [])

  const hasRole = useCallback(
    (...roles: string[]) => roles.some((role) => user?.roles.includes(role)),
    [user],
  )

  const value = useMemo(
    () => ({
      user,
      token,
      isAuthenticated: Boolean(token && user),
      login,
      register,
      logout,
      hasRole,
    }),
    [user, token, login, register, logout, hasRole],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
