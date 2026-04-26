import { TableRow, TableCell } from '@/components/ui/table'
import { Edit2, Trash2, Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import type { Column } from './Column'

interface DataRowProps<T> {
  item: T
  columns: Column<T>[]
  isDark: boolean
  isDeleting?: boolean
  onEdit?: (item: T) => void
  onDelete?: (id: number) => void
  getId?: (item: T) => number | string
  actions?: boolean
}

export function DataRow<T>({ 
  item, 
  columns, 
  isDark, 
  isDeleting = false, 
  onEdit, 
  onDelete,
  getId,
  actions = true 
}: DataRowProps<T>) {
  const id = getId ? getId(item) : (item as unknown as { id: number }).id

  return (
    <TableRow className={isDark ? 'border-white/5' : 'border-slate-100'}>
      {columns.map((column) => (
        <TableCell key={column.key}>
          {column.render 
            ? column.render(item, isDark)
            : String((item as unknown as Record<string, unknown>)[column.key] ?? '')}
        </TableCell>
      ))}
      {actions && (
        <TableCell className="text-right">
          <div className="flex items-center justify-end gap-1">
            {onEdit && (
              <Button 
                variant="ghost" 
                size="icon"
                onClick={() => onEdit(item)}
                className={`h-8 w-8 ${isDark ? 'hover:bg-white/10 text-blue-400' : 'hover:bg-slate-100 text-blue-600'}`}
                title="Edit"
              >
                <Edit2 className="h-3 w-3" />
              </Button>
            )}
            {onDelete && (
              <Button 
                variant="ghost" 
                size="icon"
                className={`h-8 w-8 ${isDark ? 'text-slate-400 hover:bg-red-500/10 hover:text-red-500' : 'text-slate-500 hover:bg-red-50 hover:text-red-500'}`}
                onClick={() => onDelete(id as number)}
                disabled={isDeleting}
                title="Delete"
              >
                {isDeleting ? <Loader2 className="h-3 w-3 animate-spin" /> : <Trash2 className="h-3 w-3" />}
              </Button>
            )}
          </div>
        </TableCell>
      )}
    </TableRow>
  )
}