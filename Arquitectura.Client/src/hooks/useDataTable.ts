import { useMemo } from 'react'
import { useDataTableConfig, type UseDataTableConfigOptions, type SortConfig } from './useDataTableConfig'
import type { ColumnConfig } from '@/services/dataTableApi'

export interface ColumnRender<T> {
  key: string
  header: string
  render?: (item: T, isDark: boolean) => React.ReactNode
}

export interface UseDataTableOptions<T> extends UseDataTableConfigOptions {
  customColumns: ColumnRender<T>[]
}

export interface UseDataTableReturn<T> {
  // DataTableConfig hook
  config: ColumnConfig[]
  visibleColumnKeys: string[]
  currentSort: SortConfig
  pageSize: number
  isColumnVisible: (key: string) => boolean
  toggleColumn: (key: string) => void
  setSort: (column: string | null, direction?: string) => void
  setPageSize: (size: number) => void
  isLoading: boolean
  isSaving: boolean
  error: Error | null
  saveConfig: () => Promise<void>
  resetConfig: () => Promise<void>
  hasUnsavedChanges: boolean

  // Computed
  columns: ColumnRender<T>[]
  allColumns: ColumnConfig[]
}

export function useDataTable<T>({ 
  tableKey, 
  pageSize, 
  customColumns, 
}: UseDataTableOptions<T>) {
  const dataTableConfig = useDataTableConfig({ tableKey, pageSize })

  // Combinar config del backend con renders custom del frontend
  const columns = useMemo(() => {
    return customColumns.filter(col => dataTableConfig.isColumnVisible(col.key))
  }, [customColumns, dataTableConfig.visibleColumnKeys])

  // Todas las columnas (para el dropdown de visibilidad)
  const allColumns = dataTableConfig.config

  return {
    ...dataTableConfig,
    columns,
    allColumns,
  }
}

// Re-export types
export type { SortConfig } from './useDataTableConfig'
export type { ColumnConfig } from '@/services/dataTableApi'