import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'

interface LoadingStateProps {
  isDark: boolean
  rows?: number
}

export function LoadingState({ isDark, rows = 5 }: LoadingStateProps) {
  return (
    <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95'}>
      <CardContent className="pt-6">
        <div className="space-y-4">
          {[...Array(rows)].map((_, i) => (
            <div key={i} className="flex items-center gap-4">
              <Skeleton className="h-4 w-12" />
              <Skeleton className="h-4 w-24" />
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-8 w-20 ml-auto" />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  )
}