import { useState, useCallback, useMemo } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { dataTableApi, type ColumnConfig, type SaveDataTableConfigRequest, type SortConfig } from '@/services/dataTableApi'

export type { SortConfig }

export interface UseDataTableConfigOptions {
  tableKey: string
  pageSize?: number
}

export interface UseDataTableConfigReturn {
  // Config
  config: ColumnConfig[]
  visibleColumnKeys: string[]
  currentSort: SortConfig
  pageSize: number
  
  // Visibility
  isColumnVisible: (key: string) => boolean
  toggleColumn: (key: string) => void
  setVisibleColumns: (keys: string[]) => void
  
  // Sort
  setSort: (column: string | null, direction?: string) => void
  
  // Page size
  setPageSize: (size: number) => void
  
  // State
  isLoading: boolean
  isSaving: boolean
  error: Error | null
  
  // Mutations
  saveConfig: () => Promise<void>
  resetConfig: () => Promise<void>
  hasUnsavedChanges: boolean
}

export function useDataTableConfig({ tableKey, pageSize = 20 }: UseDataTableConfigOptions): UseDataTableConfigReturn {
  const queryClient = useQueryClient()

  // Fetch config from server
  const { data: serverConfig, isLoading, error } = useQuery({
    queryKey: ['datatable-config', tableKey],
    queryFn: () => dataTableApi.getConfig(tableKey),
  })

  // Local state for editing
  const [visibleColumns, setVisibleColumnsState] = useState<string[]>([])
  const [currentSort, setCurrentSort] = useState<SortConfig>({ column: null, direction: 'asc' })
  const [currentPageSize, setCurrentPageSize] = useState(pageSize)
  const [hasChanges, setHasChanges] = useState(false)

  // Config from server
  const config = useMemo(() => serverConfig?.columns ?? [], [serverConfig])
  
  // Sync local state when server config loads (only if not already modified by user)
  if (serverConfig && visibleColumns.length === 0) {
    setVisibleColumnsState(serverConfig.visibleColumnKeys)
    setCurrentSort(serverConfig.sort)
    setCurrentPageSize(serverConfig.pageSize || pageSize)
  }

  // Save mutation
  const saveMutation = useMutation({
    mutationFn: (config: SaveDataTableConfigRequest) => dataTableApi.saveConfig(tableKey, config),
    onSuccess: () => {
      setHasChanges(false)
      queryClient.invalidateQueries({ queryKey: ['datatable-config', tableKey] })
    },
  })

  // Reset mutation
  const resetMutation = useMutation({
    mutationFn: () => dataTableApi.resetConfig(tableKey),
    onSuccess: () => {
      setHasChanges(false)
      queryClient.invalidateQueries({ queryKey: ['datatable-config', tableKey] })
    },
  })

  // Toggle column visibility
  const toggleColumn = useCallback((key: string) => {
    const column = config.find(c => c.key === key)
    if (!column || column.isAlwaysVisible) return

    setVisibleColumnsState(prev => {
      return prev.includes(key)
        ? prev.filter(k => k !== key)
        : [...prev, key]
    })
    setHasChanges(true)
  }, [config])

  // Set visible columns directly
  const setVisibleColumns = useCallback((keys: string[]) => {
    setVisibleColumnsState(keys)
    setHasChanges(true)
  }, [])

  // Set sort
  const setSort = useCallback((column: string | null, direction?: string) => {
    setCurrentSort({
      column,
      direction: direction ?? (column === currentSort.column && currentSort.direction === 'asc' ? 'desc' : 'asc'),
    })
    setHasChanges(true)
  }, [currentSort])

  // Set page size
  const setPageSize = useCallback((size: number) => {
    setCurrentPageSize(size)
    setHasChanges(true)
  }, [])

  // Check if column is visible
  const isColumnVisible = useCallback((key: string) => {
    return visibleColumns.includes(key)
  }, [visibleColumns])

  // Save config
  const saveConfig = useCallback(async () => {
    const saveRequest: SaveDataTableConfigRequest = {
      visibleColumns,
      activeFilters: serverConfig?.activeFilterKeys ?? [],
      filterValues: {},
      sortConfig: currentSort,
      pageSize: currentPageSize,
    }
    await saveMutation.mutateAsync(saveRequest)
  }, [visibleColumns, serverConfig, currentSort, currentPageSize, saveMutation])

  // Reset config
  const resetConfig = useCallback(async () => {
    await resetMutation.mutateAsync()
  }, [resetMutation])

  return {
    config,
    visibleColumnKeys: visibleColumns,
    currentSort,
    pageSize: currentPageSize,
    isColumnVisible,
    toggleColumn,
    setVisibleColumns,
    setSort,
    setPageSize,
    isLoading,
    isSaving: saveMutation.isPending || resetMutation.isPending,
    error: error as Error | null,
    saveConfig,
    resetConfig,
    hasUnsavedChanges: hasChanges,
  }
}