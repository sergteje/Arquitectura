import { useState, useEffect } from 'react'

interface BackgroundProps {
  children: React.ReactNode
  className?: string
  centered?: boolean
}

function getSystemTheme(): 'dark' | 'light' {
  const stored = localStorage.getItem('theme')
  if (stored === 'dark' || stored === 'light') return stored
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function Background({ children, className = '', centered = false }: BackgroundProps) {
  const [theme, setTheme] = useState<'dark' | 'light'>('light')

  useEffect(() => {
    const detected = getSystemTheme()
    setTheme(detected)
    
    const handleThemeChange = () => setTheme(getSystemTheme())
    
    window.addEventListener('theme-change', handleThemeChange)
    
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    const handler = (e: MediaQueryListEvent) => {
      if (!localStorage.getItem('theme')) {
        setTheme(e.matches ? 'dark' : 'light')
      }
    }
    mediaQuery.addEventListener('change', handler)
    
    return () => {
      window.removeEventListener('theme-change', handleThemeChange)
      mediaQuery.removeEventListener('change', handler)
    }
  }, [])

  const isDark = theme === 'dark'

  return (
    <div className={`min-h-screen relative flex ${centered ? 'items-center justify-center' : ''} ${className}`}>
      {/* Animated background */}
      <div className={`fixed inset-0 transition-colors duration-500 ${
        isDark 
          ? 'bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900' 
          : 'bg-gradient-to-br from-slate-200 via-slate-300 to-slate-400'
      }`}>
        <div className={`absolute inset-0 ${isDark ? 'opacity-20' : 'opacity-30'}`}>
          <div className={`absolute top-0 left-1/3 w-[500px] h-[500px] rounded-full blur-3xl animate-pulse ${
            isDark ? 'bg-blue-500/20' : 'bg-blue-500/20'
          }`} />
          <div className={`absolute bottom-0 right-1/3 w-[500px] h-[500px] rounded-full blur-3xl animate-pulse delay-1000 ${
            isDark ? 'bg-cyan-500/20' : 'bg-cyan-500/20'
          }`} />
        </div>
      </div>
      {/* Content wrapper */}
      <div className="relative z-10 w-full">
        {children}
      </div>
    </div>
  )
}