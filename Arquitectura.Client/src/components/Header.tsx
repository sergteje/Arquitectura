import { useState } from 'react'
import { Cloud, LogOut, Menu, X } from 'lucide-react'
import { useTheme } from 'next-themes'
import { Button } from '@/components/ui/button'
import { ThemeToggle } from './ThemeToggle'
import { APP_NAME, APP_TAGLINE } from '@/lib/constants'

interface HeaderProps {
  user?: { firstName: string; lastName: string } | null
  onLogout: () => void
}

export function Header({ user, onLogout }: HeaderProps) {
  const { theme } = useTheme()
  const [menuOpen, setMenuOpen] = useState(false)
  const isDark = theme === 'dark'

  return (
    <>
      <header className="sticky top-0 z-50 w-full backdrop-blur-xl border-b border-white/10">
        <div className="container flex h-16 items-center justify-between mx-auto px-4 max-w-6xl">
          {/* Logo */}
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 text-white shadow-lg shadow-blue-500/30">
              <Cloud className="h-5 w-5" />
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

          {/* Desktop Navigation */}
          <div className={`hidden md:flex items-center gap-3 ${isDark ? 'text-slate-200' : 'text-slate-700'}`}>
            {user && (
              <span className="text-sm font-medium">
                {user.firstName} {user.lastName}
              </span>
            )}
            <Button 
              variant="ghost" 
              size="icon"
              onClick={onLogout}
              title="Logout"
              className={`h-9 w-9 ${isDark ? 'hover:bg-white/10' : 'hover:bg-slate-300'}`}
            >
              <LogOut className="h-4 w-4" />
            </Button>
            <ThemeToggle />
          </div>

          {/* Mobile Menu Button */}
          <div className="md:hidden flex items-center gap-2">
            <ThemeToggle />
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setMenuOpen(!menuOpen)}
              className={isDark ? 'text-white' : 'text-slate-900'}
            >
              {menuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </Button>
          </div>
        </div>

        {/* Mobile Menu */}
        {menuOpen && (
          <div className={`md:hidden border-t ${isDark ? 'border-white/10 bg-black/20' : 'border-slate-300/50 bg-white/50'}`}>
            <div className="container px-4 py-4 space-y-3">
              {user && (
                <div className={`text-sm px-3 py-2 ${isDark ? 'text-slate-300' : 'text-slate-700'}`}>
                  {user.firstName} {user.lastName}
                </div>
              )}
              <Button 
                variant="ghost"
                className={`w-full justify-start ${
                  isDark ? 'hover:bg-white/10' : 'hover:bg-slate-200'
                }`}
                onClick={() => {
                  onLogout()
                  setMenuOpen(false)
                }}
              >
                <LogOut className="h-4 w-4 mr-2" />
                Logout
              </Button>
            </div>
          </div>
        )}
      </header>
    </>
  )
}