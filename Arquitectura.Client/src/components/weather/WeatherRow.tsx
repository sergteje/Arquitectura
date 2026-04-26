import { Edit2, Trash2, Loader2 } from 'lucide-react'
import type { WeatherForecast } from '@/services/weatherApi'
import { Button } from '@/components/ui/button'
import { TableRow, TableCell } from '@/components/ui/table'

interface WeatherRowProps {
  forecast: WeatherForecast
  isDark: boolean
  isDeleting: boolean
  onEdit: (forecast: WeatherForecast) => void
  onDelete: (id: number) => void
}

export function WeatherRow({ forecast, isDark, isDeleting, onEdit, onDelete }: WeatherRowProps) {
  return (
    <TableRow className={`${isDark ? 'border-white/5' : 'border-slate-100'}`}>
      {/* ID */}
      <TableCell>
        <span className={`inline-flex px-2 py-1 rounded-md text-xs font-medium ${
          isDark ? 'bg-slate-800 text-slate-300' : 'bg-slate-100 text-slate-700'
        }`}>
          #{forecast.id}
        </span>
      </TableCell>

      {/* Date */}
      <TableCell>
        <span className={isDark ? 'bg-blue-500/20 text-blue-300 px-2 py-1 rounded-md' : 'bg-blue-50 text-blue-700 px-2 py-1 rounded-md'}>
          {forecast.date}
        </span>
      </TableCell>

      {/* Temperature */}
      <TableCell>
        <span className={`font-bold text-lg ${
          forecast.temperatureC > 20 
            ? isDark ? 'text-red-400' : 'text-red-600'
            : forecast.temperatureC < 5 
              ? isDark ? 'text-blue-400' : 'text-blue-600'
              : isDark ? 'text-yellow-400' : 'text-yellow-600'
        }`}>
          {forecast.temperatureC}°C
        </span>
        <span className={isDark ? 'text-slate-500' : 'text-slate-400'}> /{forecast.temperatureF}°F</span>
      </TableCell>

      {/* Summary */}
      <TableCell>
        {forecast.summary ? (
          <div className="flex items-center gap-2">
            <span className={isDark ? 'text-slate-300' : 'text-slate-700'}>{forecast.summary}</span>
          </div>
        ) : (
          <span className={isDark ? 'text-slate-500 italic' : 'text-slate-400 italic'}>No summary</span>
        )}
      </TableCell>

      {/* Actions */}
      <TableCell className="text-right">
        <div className="flex items-center justify-end gap-1">
          <Button 
            variant="ghost" 
            size="icon"
            onClick={() => onEdit(forecast)}
            className={`h-8 w-8 ${isDark ? 'hover:bg-white/10 text-blue-400' : 'hover:bg-slate-100 text-blue-600'}`}
            title="Edit"
          >
            <Edit2 className="h-3 w-3" />
          </Button>
          <Button 
            variant="ghost" 
            size="icon"
            className={`h-8 w-8 ${isDark ? 'text-slate-400 hover:bg-red-500/10 hover:text-red-500' : 'text-slate-500 hover:bg-red-50 hover:text-red-500'}`}
            onClick={() => onDelete(forecast.id)}
            disabled={isDeleting}
            title="Delete"
          >
            {isDeleting ? <Loader2 className="h-3 w-3 animate-spin" /> : <Trash2 className="h-3 w-3" />}
          </Button>
        </div>
      </TableCell>
    </TableRow>
  )
}