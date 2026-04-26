import { Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface EmptyStateProps {
  isDark: boolean
  title?: string
  description?: string
  actionLabel?: string
  onAction?: () => void
}

export function EmptyState({ 
  isDark, 
  title = 'No data found',
  description = 'Get started by creating your first item.',
  actionLabel = 'Create',
  onAction 
}: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16">
      <p className={`text-lg font-medium ${isDark ? 'text-slate-400' : 'text-slate-600'}`}>
        {title}
      </p>
      <p className={`text-sm ${isDark ? 'text-slate-500' : 'text-slate-500'} mb-4`}>
        {description}
      </p>
      {onAction && (
        <Button variant="outline" size="sm" onClick={onAction}>
          <Plus className="mr-2 h-4 w-4" />
          {actionLabel}
        </Button>
      )}
    </div>
  )
}