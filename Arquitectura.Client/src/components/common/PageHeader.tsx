import { Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface PageHeaderProps {
  title: string
  description?: string
  actionLabel?: string
  onAction?: () => void
  isDark: boolean
}

export function PageHeader({ title, description, actionLabel, onAction, isDark }: PageHeaderProps) {
  return (
    <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h2 className={`text-3xl font-bold tracking-tight ${isDark ? 'text-white' : 'text-slate-950'}`}>
          {title}
        </h2>
        {description && (
          <p className={`mt-1 ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>
            {description}
          </p>
        )}
      </div>
      {onAction && actionLabel && (
        <Button 
          onClick={onAction} 
          size="lg" 
          className="bg-blue-600 hover:bg-blue-700 text-white border-0 shadow-lg shadow-blue-500/30"
        >
          <Plus className="mr-2 h-4 w-4" />
          {actionLabel}
        </Button>
      )}
    </div>
  )
}