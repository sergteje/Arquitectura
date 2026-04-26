import { Card, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'
import { Button } from '@/components/ui/button'

interface ErrorStateProps {
  isDark: boolean
}

export function ErrorState({ isDark }: ErrorStateProps) {
  return (
    <Card className="border-red-500/50 bg-red-500/10">
      <CardHeader>
        <CardTitle className="text-red-500">Error</CardTitle>
        <CardDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
          Failed to load weather forecasts.
        </CardDescription>
      </CardHeader>
      <CardFooter>
        <Button variant="outline" onClick={() => window.location.reload()}>Retry</Button>
      </CardFooter>
    </Card>
  )
}