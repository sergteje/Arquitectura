import { useEffect } from 'react'
import { Moon, Sun } from 'lucide-react'
import { useTheme } from 'next-themes'
import { Button } from '@/components/ui/button'

export function ThemeToggle() {
  const { theme, setTheme, resolvedTheme } = useTheme()

  // Ensure theme is set on mount
  useEffect(() => {
    if (!theme) {
      setTheme('system')
    }
  }, [theme, setTheme])

  const toggleTheme = () => {
    const newTheme = resolvedTheme === 'dark' ? 'light' : 'dark'
    setTheme(newTheme)
  }

  const isDark = resolvedTheme === 'dark'

  return (
    <Button
      variant="ghost"
      size="icon"
      onClick={toggleTheme}
      className={`h-10 w-10 rounded-full border transition-all ${
        isDark 
          ? 'border-white/20 bg-white/10 text-white hover:bg-white/20' 
          : 'border-slate-400/50 bg-white/80 hover:bg-white text-slate-700 hover:text-slate-900'
      }`}
    >
      <Sun className={`h-4 w-4 transition-all absolute ${isDark ? 'opacity-0 rotate-90 scale-0' : 'opacity-100 rotate-0 scale-100'}`} />
      <Moon className={`h-4 w-4 transition-all ${isDark ? 'opacity-100 rotate-0 scale-100' : 'opacity-0 -rotate-90 scale-0'}`} />
      <span className="sr-only">Toggle theme</span>
    </Button>
  )
}