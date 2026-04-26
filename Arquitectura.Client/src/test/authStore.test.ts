import { describe, it, expect, beforeEach } from 'vitest'
import { useAuthStore } from '../stores/authStore'

describe('useAuthStore', () => {
  beforeEach(() => {
    // Reset store
    useAuthStore.getState().logout()
    localStorage.clear()
  })

  it('should login and set user', () => {
    const login = useAuthStore.getState().login
    
    const result = login({
      user: { id: 1, email: 'test@test.com', firstName: 'John', lastName: 'Doe' },
      accessToken: 'token123',
      refreshToken: 'refresh123',
      expiresAt: '2026-04-27',
    })
    
    const state = useAuthStore.getState()
    
    expect(state.isAuthenticated).toBe(true)
    expect(state.user?.email).toBe('test@test.com')
    expect(state.accessToken).toBe('token123')
  })

  it('should logout and clear state', () => {
    const login = useAuthStore.getState().login
    const logout = useAuthStore.getState().logout
    
    login({
      user: { id: 1, email: 'test@test.com', firstName: 'John', lastName: 'Doe' },
      accessToken: 'token123',
      refreshToken: 'refresh123',
      expiresAt: '2026-04-27',
    })
    
    logout()
    
    const state = useAuthStore.getState()
    expect(state.isAuthenticated).toBe(false)
    expect(state.user).toBeNull()
  })

  it('should check if token is expired', () => {
    const login = useAuthStore.getState().login
    const isTokenExpired = useAuthStore.getState().isTokenExpired
    
    // Login with valid token (expires in the future)
    login({
      user: { id: 1, email: 'test@test.com', firstName: 'John', lastName: 'Doe' },
      accessToken: 'token123',
      refreshToken: 'refresh123',
      expiresAt: new Date(Date.now() + 3600000).toISOString(), // +1 hour
    })
    
    expect(isTokenExpired()).toBe(false)
  })
})