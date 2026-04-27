import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Settings, Save, RotateCcw, Loader2 } from 'lucide-react'
import { toast } from 'sonner'
import type { ColumnConfig } from '@/services/dataTableApi'

interface DataTableToolbarProps {
  columns: ColumnConfig[]
  visibleColumns: string[]
  onToggleColumn: (key: string) => void
  onSaveConfig: () => Promise<void>
  onResetConfig: () => Promise<void>
  hasUnsavedChanges: boolean
  isSaving: boolean
  isDark: boolean
}

export function DataTableToolbar({
  columns,
  visibleColumns,
  onToggleColumn,
  onSaveConfig,
  onResetConfig,
  hasUnsavedChanges,
  isSaving,
  isDark,
}: DataTableToolbarProps) {
  const [isSavingLocal, setIsSavingLocal] = useState(false)

  const handleSave = async () => {
    setIsSavingLocal(true)
    try {
      await onSaveConfig()
      toast.success('Configuración guardada')
    } catch {
      toast.error('Error al guardar configuración')
    } finally {
      setIsSavingLocal(false)
    }
  }

  const handleReset = async () => {
    setIsSavingLocal(true)
    try {
      await onResetConfig()
      toast.success('Configuración reseteada')
    } catch {
      toast.error('Error al resetear configuración')
    } finally {
      setIsSavingLocal(false)
    }
  }

  const alwaysVisibleColumns = columns.filter(c => c.isAlwaysVisible)
  const toggleableColumns = columns.filter(c => !c.isAlwaysVisible)

  return (
    <div className="flex items-center gap-2">
      {/* Save button */}
      <Button
        variant="outline"
        size="sm"
        onClick={handleSave}
        disabled={!hasUnsavedChanges || isSaving || isSavingLocal}
        className={`gap-1 ${isDark ? 'border-white/20 hover:bg-white/10' : 'border-slate-300'}`}
      >
        {isSavingLocal ? (
          <Loader2 className="h-3 w-3 animate-spin" />
        ) : (
          <Save className="h-3 w-3" />
        )}
        Guardar
      </Button>

      {/* Reset button */}
      <Button
        variant="outline"
        size="sm"
        onClick={handleReset}
        disabled={isSaving || isSavingLocal}
        className={`gap-1 ${isDark ? 'border-white/20 hover:bg-white/10' : 'border-slate-300'}`}
      >
        <RotateCcw className="h-3 w-3" />
        Resetear
      </Button>

      {/* Column visibility dropdown */}
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            className={`gap-1 ${isDark ? 'border-white/20 hover:bg-white/10' : 'border-slate-300'}`}
          >
            <Settings className="h-3 w-3" />
            Columnas
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent 
          align="end" 
          className={isDark ? 'bg-slate-900 border-white/20' : 'bg-white'}
        >
          <DropdownMenuLabel>Visibilidad de columnas</DropdownMenuLabel>
          <DropdownMenuSeparator className={isDark ? 'bg-white/10' : 'bg-slate-200'} />
          
          {/* Always visible columns (info only) */}
          {alwaysVisibleColumns.length > 0 && (
            <>
              <DropdownMenuItem className="opacity-50 cursor-default" disabled>
                <span className="text-xs mr-2">Siempre visibles:</span>
              </DropdownMenuItem>
              {alwaysVisibleColumns.map(col => (
                <DropdownMenuItem key={col.key} className="opacity-50 cursor-default" disabled>
                  <Checkbox 
                    checked={true} 
                    disabled 
                    className="mr-2" 
                  />
                  {col.label}
                </DropdownMenuItem>
              ))}
              <DropdownMenuSeparator className={isDark ? 'bg-white/10' : 'bg-slate-200'} />
            </>
          )}

          {/* Toggleable columns */}
          {toggleableColumns.map(col => {
            const isVisible = visibleColumns.includes(col.key)
            return (
              <DropdownMenuItem
                key={col.key}
                onClick={() => onToggleColumn(col.key)}
                className="cursor-pointer"
              >
                <Checkbox 
                  checked={isVisible} 
                  className="mr-2" 
                />
                {col.label}
              </DropdownMenuItem>
            )
          })}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  )
}