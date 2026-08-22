import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { api } from './api'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [config, setConfig] = useState({ googleEnabled: false, devFakeAuth: false })
  const [loading, setLoading] = useState(true)

  const refresh = useCallback(async () => {
    try {
      const me = await api.get('/api/auth/me')
      setUser(me.authenticated ? me : null)
    } catch {
      setUser(null)
    }
  }, [])

  useEffect(() => {
    let cancelled = false
    async function init() {
      try {
        const cfg = await api.get('/api/auth/config')
        if (!cancelled) setConfig(cfg)
      } catch {
        // API down — landing page still renders
      }
      await refresh()
      if (!cancelled) setLoading(false)
    }
    init()
    return () => { cancelled = true }
  }, [refresh])

  const devLogin = useCallback(async () => {
    await api.post('/api/auth/dev-login')
    await refresh()
  }, [refresh])

  const logout = useCallback(async () => {
    await api.post('/api/auth/logout')
    setUser(null)
  }, [])

  const acceptConsent = useCallback(async () => {
    await api.post('/api/auth/consent')
    await refresh()
  }, [refresh])

  return (
    <AuthContext.Provider value={{ user, config, loading, refresh, devLogin, logout, acceptConsent }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}

// Wraps private pages: wait for the session check, then either render or
// bounce to the landing page.
export function RequireAuth({ children }) {
  const { user, loading } = useAuth()
  const location = useLocation()
  if (loading) return <p className="page-status">Loading…</p>
  if (!user) return <Navigate to="/" replace state={{ from: location.pathname }} />
  return children
}
