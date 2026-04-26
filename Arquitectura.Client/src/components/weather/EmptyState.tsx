import { Plus, Cloud } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface EmptyStateProps {
  isDark: boolean
  onCreate: () => void
}

export function EmptyState({ isDark, onCreate }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-16">
      <div className={`p-4 rounded-full ${isDark ? 'bg-slate-800' : 'bg-slate-100'} mb-4`}>
        <Cloud className={`h-8 w-8 ${isDark ? 'text-slate-500' : 'text-slate-400'}`} />
      </div>
      <p className={`text-lg font-medium ${isDark ? 'text-slate-400' : 'text-slate-600'}`}>
        No forecasts found
      </p>
      <p className={`text-sm ${isDark ? 'text-slate-500' : 'text-slate-500'} mb-4`}>
        Get started by creating your first forecast
      </p>
      <Button variant="outline" size="sm" onClick={onCreate}>
        <Plus className="mr-2 h-4 w-4" />
        Create your first forecast
      </Button>
    </div>
  )
}