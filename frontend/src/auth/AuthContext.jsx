import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { apiClient } from '../api/client'

const TOKEN_STORAGE_KEY = 'issue-tracker-access-token'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [accessToken, setAccessToken] = useState(() => localStorage.getItem(TOKEN_STORAGE_KEY))
  const [user, setUser] = useState(null)
  const [isLoading, setIsLoading] = useState(Boolean(localStorage.getItem(TOKEN_STORAGE_KEY)))

  useEffect(() => {
    let isActive = true

    async function loadCurrentUser() {
      if (!accessToken) {
        setUser(null)
        setIsLoading(false)
        return
      }

      setIsLoading(true)

      try {
        const currentUser = await apiClient.get('/auth/me', {
          accessToken,
        })

        if (isActive) {
          setUser(currentUser)
        }
      } catch {
        if (isActive) {
          localStorage.removeItem(TOKEN_STORAGE_KEY)
          setAccessToken(null)
          setUser(null)
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    loadCurrentUser()

    return () => {
      isActive = false
    }
  }, [accessToken])

  const value = useMemo(
    () => ({
      accessToken,
      isAuthenticated: Boolean(accessToken && user),
      isLoading,
      user,
      async login(credentials) {
        const response = await apiClient.post('/auth/login', credentials)
        localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken)
        setAccessToken(response.accessToken)
        setUser(response.user)
      },
      async register(payload) {
        const response = await apiClient.post('/auth/register', payload)
        localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken)
        setAccessToken(response.accessToken)
        setUser(response.user)
      },
      logout() {
        localStorage.removeItem(TOKEN_STORAGE_KEY)
        setAccessToken(null)
        setUser(null)
      },
    }),
    [accessToken, isLoading, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }

  return context
}
