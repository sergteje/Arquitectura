import { Card, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'

interface ErrorStateProps {
  isDark: boolean
  title?: string
  message?: string
  onRetry?: () => void
}

export function ErrorState({ 
  isDark, 
  title = 'Error', 
  message = 'Something went wrong.',
  onRetry 
}: ErrorStateProps) {
  return (
    <Card className="border-red-500/50 bg-red-500/10">
      <CardHeader>
        <CardTitle className="text-red-500">{title}</CardTitle>
        <CardDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
          {message}
        </CardDescription>
      </CardHeader>
      <CardFooter>
        <Button variant="outline" onClick={onRetry || (() => window.location.reload())}>
          Retry
        </Button>
      </CardFooter>
    </Card>
  )
}