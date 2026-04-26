import { useTheme } from 'next-themes'
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'

interface GlassCardProps {
  children: React.ReactNode
  className?: string
  title?: string
  description?: string
}

export function GlassCard({ children, className = '', title, description }: GlassCardProps) {
  const { theme } = useTheme()
  const isDark = theme === 'dark'

  return (
    <Card className={`${isDark ? 'bg-white/5 border-white/10' : 'bg-white/95 border-slate-300/50'} backdrop-blur-xl ${className}`}>
      {title && (
        <CardHeader>
          <CardTitle className={isDark ? 'text-white' : 'text-slate-950'}>
            {title}
          </CardTitle>
          {description && (
            <CardDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
              {description}
            </CardDescription>
          )}
        </CardHeader>
      )}
      <CardContent>{children}</CardContent>
    </Card>
  )
}

// Re-export sub-components for convenience
export const GlassCardContent = CardContent
export const GlassCardHeader = CardHeader
export const GlassCardTitle = CardTitle
export const GlassCardDescription = CardDescription
export const GlassCardFooter = CardFooter