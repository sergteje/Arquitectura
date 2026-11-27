import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { toast } from 'sonner'
import { LogIn, UserPlus, ArrowRight } from 'lucide-react'
import { useTheme } from 'next-themes'
import { Background } from '@/components/Background'
import { ThemeToggle } from '@/components/ThemeToggle'
import { APP_NAME, APP_TAGLINE, API_BASE } from '@/lib/constants'
import { problemMessage } from '@/lib/problem'
import { useAuthStore } from '@/stores/authStore'

interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: { id: number; email: string; firstName: string; lastName: string }
}

type AuthMode = 'login' | 'register'

export default function Login() {
  const [mode, setMode] = useState<AuthMode>('login')
  const [loading, setLoading] = useState(false)
  const { resolvedTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  // Login fields
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')

  // Register fields
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [registerEmail, setRegisterEmail] = useState('')
  const [registerPassword, setRegisterPassword] = useState('')

  const navigate = useNavigate()
  
  // Zustand store
  const login = useAuthStore((state) => state.login)

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      const response = await fetch(`${API_BASE}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })
      const data = await response.json()
      if (!response.ok) {
        toast.error(problemMessage(data, 'Login failed'))
        return
      }
      const result = data as LoginResponse
      
      // Use Zustand store instead of localStorage
      login({
        user: result.user,
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
        expiresAt: result.expiresAt,
      })
      
      toast.success(`Welcome back, ${result.user.firstName}!`)
      navigate('/')
    } catch {
      toast.error('An error occurred during login')
    } finally {
      setLoading(false)
    }
  }

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      const response = await fetch(`${API_BASE}/api/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: registerEmail, password: registerPassword, firstName, lastName }),
      })
      const data = await response.json()
      if (!response.ok) {
        toast.error(problemMessage(data, 'Registration failed'))
        return
      }
      const result = data as LoginResponse
      
      // Use Zustand store instead of localStorage
      login({
        user: result.user,
        accessToken: result.accessToken,
        refreshToken: result.refreshToken,
        expiresAt: result.expiresAt,
      })
      
      toast.success(`Welcome, ${result.user.firstName}!`)
      navigate('/')
    } catch {
      toast.error('An error occurred during registration')
    } finally {
      setLoading(false)
    }
  }

  const inputClass = isDark 
    ? 'bg-white/5 border-white/20 text-white placeholder:text-slate-500 focus:border-blue-500' 
    : 'bg-slate-50 border-slate-300 text-slate-900 placeholder:text-slate-400 focus:border-blue-500'

  return (
    <Background>
      {/* Header - fixed at top */}
      <header className={`sticky top-0 z-50 w-full backdrop-blur-xl border-b ${
        isDark ? 'bg-white/5 border-white/10' : 'bg-white/80 border-slate-300/50'
      }`}>
        <div className="container flex h-16 items-center justify-between mx-auto px-4 max-w-6xl">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 text-white shadow-lg shadow-blue-500/30">
              <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 15a4 4 0 004 4h9a5 5 0 10-.1-9.999 5.002 5.002 0 10-9.9 9.999A4 4 0 003 15z" />
              </svg>
            </div>
            <div>
              <h1 className={`text-xl font-bold tracking-tight ${isDark ? 'text-white' : 'text-slate-950'}`}>
                {APP_NAME}
              </h1>
              <p className={`text-xs ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>
                {APP_TAGLINE}
              </p>
            </div>
          </div>
          <ThemeToggle />
        </div>
      </header>

      {/* Main content area - below header */}
      <main className="container mx-auto px-4 py-16 max-w-6xl">
        {/* Auth Card - centered */}
        <Card className={`w-[420px] mx-auto shadow-2xl transition-all duration-500 ${
          isDark ? 'bg-white/10 backdrop-blur-xl border-white/20' : 'bg-white/95 backdrop-blur-xl border-slate-300/50'
        }`}>
        <CardHeader className="space-y-1 text-center">
          <div className="flex justify-center gap-2 mb-4">
            <Button
              variant={mode === 'login' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => setMode('login')}
              className={`${mode === 'login' ? 'bg-blue-600 hover:bg-blue-700' : ''} ${
                isDark ? 'text-slate-300 hover:text-white hover:bg-white/10' : 'text-slate-700 hover:text-slate-900 hover:bg-slate-200'
              } transition-all`}
            >
              <LogIn className="w-4 h-4 mr-2" />
              Sign In
            </Button>
            <Button
              variant={mode === 'register' ? 'default' : 'ghost'}
              size="sm"
              onClick={() => setMode('register')}
              className={`${mode === 'register' ? 'bg-blue-600 hover:bg-blue-700' : ''} ${
                isDark ? 'text-slate-300 hover:text-white hover:bg-white/10' : 'text-slate-700 hover:text-slate-900 hover:bg-slate-200'
              } transition-all`}
            >
              <UserPlus className="w-4 h-4 mr-2" />
              Register
            </Button>
          </div>
          <CardTitle className={`text-3xl font-bold ${isDark ? 'text-white' : 'text-slate-950'}`}>
            {mode === 'login' ? 'Welcome Back' : 'Create Account'}
          </CardTitle>
          <CardDescription className={isDark ? 'text-slate-300' : 'text-slate-700'}>
            {mode === 'login' ? 'Enter your credentials' : 'Join us and start building'}
          </CardDescription>
        </CardHeader>

        <CardContent>
          {mode === 'login' ? (
            <form onSubmit={handleLogin} className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="email" className={isDark ? 'text-slate-200' : 'text-slate-700'}>Email</Label>
                <Input id="email" type="email" autoComplete="email" placeholder="name@example.com" value={email} onChange={(e) => setEmail(e.target.value)} className={inputClass} required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="password" className={isDark ? 'text-slate-200' : 'text-slate-700'}>Password</Label>
                <Input 
                  id="password" 
                  type="password" 
                  autoComplete="current-password"
                  value={password} 
                  onChange={(e) => setPassword(e.target.value)} 
                  className={inputClass} 
                  required 
                />
              </div>
              <Button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white border-0" disabled={loading}>
                {loading ? 'Signing in...' : 'Sign In'}
                <ArrowRight className="w-4 h-4 ml-2" />
              </Button>
            </form>
          ) : (
            <form onSubmit={handleRegister} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="firstName" className={isDark ? 'text-slate-200' : 'text-slate-700'}>First Name</Label>
                  <Input id="firstName" type="text" placeholder="John" value={firstName} onChange={(e) => setFirstName(e.target.value)} className={inputClass} required />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="lastName" className={isDark ? 'text-slate-200' : 'text-slate-700'}>Last Name</Label>
                  <Input id="lastName" type="text" placeholder="Doe" value={lastName} onChange={(e) => setLastName(e.target.value)} className={inputClass} required />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="registerEmail" className={isDark ? 'text-slate-200' : 'text-slate-700'}>Email</Label>
                <Input id="registerEmail" type="email" placeholder="name@example.com" value={registerEmail} onChange={(e) => setRegisterEmail(e.target.value)} className={inputClass} required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="registerPassword" className={isDark ? 'text-slate-200' : 'text-slate-700'}>Password</Label>
                <Input 
                  id="registerPassword" 
                  type="password" 
                  autoComplete="new-password"
                  value={registerPassword} 
                  onChange={(e) => setRegisterPassword(e.target.value)} 
                  className={inputClass} 
                  required 
                />
              </div>
              <Button type="submit" className="w-full bg-blue-600 hover:bg-blue-700 text-white border-0" disabled={loading}>
                {loading ? 'Creating account...' : 'Create Account'}
                <ArrowRight className="w-4 h-4 ml-2" />
              </Button>
            </form>
          )}
        </CardContent>
      </Card>
      </main>
    </Background>
  )
}