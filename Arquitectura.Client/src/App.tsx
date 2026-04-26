import { useState, useEffect } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { WeatherList } from './pages/WeatherList'
import { WeatherForm } from './pages/WeatherForm'
import Login from './pages/Login'
import type { WeatherForecast } from './services/weatherApi'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog'
import { Toaster } from '@/components/ui/sonner'
import { useTheme } from 'next-themes'
import { Background } from '@/components/Background'
import { Header } from '@/components/Header'

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:8081'

// Auth helpers
function getAuthToken(): string | null {
  return localStorage.getItem('accessToken')
}

function getCurrentUser(): { firstName: string; lastName: string } | null {
  const userStr = localStorage.getItem('user')
  return userStr ? JSON.parse(userStr) : null
}

function logout() {
  localStorage.removeItem('accessToken')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('user')
  window.location.href = '/login'
}

// Protected route wrapper
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const token = getAuthToken()
  if (!token) {
    return <Navigate to="/login" replace />
  }
  return <>{children}</>
}

// Login page - redirect if already authenticated
function LoginPage() {
  const token = getAuthToken()
  if (token) {
    return <Navigate to="/" replace />
  }
  return <Login />
}

function AppContent() {
  const [modalOpen, setModalOpen] = useState(false)
  const [selectedForecast, setSelectedForecast] = useState<WeatherForecast | null>(null)
  const [user, setUser] = useState<{ firstName: string; lastName: string } | null>(null)
  const { theme } = useTheme()

  useEffect(() => {
    setUser(getCurrentUser())
  }, [])

  const isEditMode = selectedForecast !== null

  const handleCreate = () => {
    setSelectedForecast(null)
    setModalOpen(true)
  }

  const handleEdit = (forecast: WeatherForecast) => {
    setSelectedForecast(forecast)
    setModalOpen(true)
  }

  const handleSuccess = () => {
    setModalOpen(false)
    setSelectedForecast(null)
  }

  const handleCancel = () => {
    setModalOpen(false)
    setSelectedForecast(null)
  }

  return (
    <Background>
      <Header user={user} onLogout={logout} />
      
      <main className="container mx-auto py-8 px-4 max-w-6xl relative">
        <div className="animate-in fade-in zoom-in-95 duration-500">
          <WeatherList onCreate={handleCreate} onEdit={handleEdit} />
        </div>
      </main>

      <Dialog open={modalOpen} onOpenChange={(open) => !open && handleCancel()}>
        <DialogContent className={theme === 'dark' ? 'bg-slate-900 border-slate-700' : 'bg-white border-slate-300'}>
          <DialogHeader>
            <DialogTitle className={theme === 'dark' ? 'text-white' : 'text-slate-950'}>
              {isEditMode ? 'Edit Forecast' : 'New Forecast'}
            </DialogTitle>
            <DialogDescription className={theme === 'dark' ? 'text-slate-400' : 'text-slate-700'}>
              {isEditMode ? 'Update the weather forecast details below' : 'Fill in the details for your new weather forecast'}
            </DialogDescription>
          </DialogHeader>
          <WeatherForm forecast={selectedForecast} onCancel={handleCancel} onSuccess={handleSuccess} />
        </DialogContent>
      </Dialog>

      <Toaster />
    </Background>
  )
}

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/*" element={<ProtectedRoute><AppContent /></ProtectedRoute>} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
export { API_URL }