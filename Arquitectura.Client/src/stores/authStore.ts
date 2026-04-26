import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface User {
  id: number
  email: string
  firstName: string
  lastName: string
}

interface AuthState {
  // State
  user: User | null
  accessToken: string | null
  refreshToken: string | null
  expiresAt: string | null
  isAuthenticated: boolean

  // Actions
  login: (data: {
    user: User
    accessToken: string
    refreshToken: string
    expiresAt: string
  }) => void

  logout: () => void

  // Helpers
  isTokenExpired: () => boolean
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      // Initial state
      user: null,
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      isAuthenticated: false,

      // Login action
      login: (data) => {
        set({
          user: data.user,
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
          expiresAt: data.expiresAt,
          isAuthenticated: true,
        })
      },

      // Logout action
      logout: () => {
        set({
          user: null,
          accessToken: null,
          refreshToken: null,
          expiresAt: null,
          isAuthenticated: false,
        })
      },

      // Check if token is expired
      isTokenExpired: () => {
        const { expiresAt } = get()
        if (!expiresAt) return true
        
        const expiryDate = new Date(expiresAt)
        const now = new Date()
        
        // Consider expired 30 seconds before actual expiry
        const bufferMs = 30 * 1000
        return now.getTime() > expiryDate.getTime() - bufferMs
      },
    }),
    {
      name: 'auth-storage', // unique name for localStorage
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        expiresAt: state.expiresAt,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
)

// Selector helpers
export const selectUser = (state: AuthState) => state.user
export const selectAccessToken = (state: AuthState) => state.accessToken
export const selectIsAuthenticated = (state: AuthState) => state.isAuthenticated
export const selectIsTokenExpired = (state: AuthState) => state.isTokenExpired()