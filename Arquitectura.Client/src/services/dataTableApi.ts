import { useAuthStore } from '@/stores/authStore'
import { problemMessage } from '@/lib/problem'

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:8081'

// ==================== TYPES ====================

export interface ColumnConfig {
  key: string
  label: string
  dataType: string
  sortable: boolean
  filterable: boolean
  isVisibleByDefault: boolean
  isFilterVisibleByDefault: boolean
  isAlwaysVisible: boolean
  sortOrder: number
  width: number | null
  filterType: string | null
  filterOptionsSource: string | null
}

export interface SortConfig {
  column: string | null
  direction: string
}

export interface DataTableConfigResponse {
  tableId: number
  columns: ColumnConfig[]
  visibleColumnKeys: string[]
  availableFilters: string[]
  activeFilterKeys: string[]
  currentFilters: Record<string, unknown>
  sort: SortConfig
  pageSize: number
}

export interface SaveDataTableConfigRequest {
  visibleColumns: string[]
  activeFilters: string[]
  filterValues: Record<string, unknown>
  sortConfig: SortConfig
  pageSize: number
}

export interface DataTableRequest {
  page: number
  pageSize: number
  sort: string | null
  direction: string
  filters: Record<string, unknown>
}

export interface DataResponse<T> {
  data: T[]
  total: number
  page: number
  pageSize: number
}

export interface DataTableExportRequest {
  filters: Record<string, unknown>
  sortColumn: string | null
  sortDirection: string
}

export interface DataTableExportResponse {
  requestId: string
  message: string
  downloadUrl: string | null
  fileName: string | null
}

// ==================== API FUNCTIONS ====================

export const dataTableApi = {
  getConfig: async (tableKey: string): Promise<DataTableConfigResponse> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/datatable/${tableKey}`, {
      headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {},
    })
    if (!res.ok) {
      const error = await res.json().catch(() => null)
      throw new Error(problemMessage(error, 'Failed to fetch config'))
    }
    return res.json()
  },

  saveConfig: async (tableKey: string, config: SaveDataTableConfigRequest): Promise<void> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/datatable/${tableKey}/config`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {}),
      },
      body: JSON.stringify(config),
    })
    if (!res.ok && res.status !== 204) {
      const error = await res.json().catch(() => null)
      throw new Error(problemMessage(error, 'Failed to save config'))
    }
  },

  resetConfig: async (tableKey: string): Promise<void> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/datatable/${tableKey}/config`, {
      method: 'DELETE',
      headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {},
    })
    if (!res.ok && res.status !== 204) {
      const error = await res.json().catch(() => null)
      throw new Error(problemMessage(error, 'Failed to reset config'))
    }
  },

  getData: async (tableKey: string, request: DataTableRequest): Promise<DataResponse<Record<string, unknown>>> => {
    const accessToken = useAuthStore.getState().accessToken
    const params = new URLSearchParams({
      page: request.page.toString(),
      pageSize: request.pageSize.toString(),
      direction: request.direction,
    })
    if (request.sort) params.append('sort', request.sort)
    if (Object.keys(request.filters).length > 0) {
      params.append('filters', JSON.stringify(request.filters))
    }

    const res = await fetch(`${API_BASE}/api/datatable/${tableKey}/data?${params}`, {
      headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {},
    })
    if (!res.ok) {
      const error = await res.json().catch(() => null)
      throw new Error(problemMessage(error, 'Failed to fetch data'))
    }
    return res.json()
  },

  createExport: async (tableKey: string, request: DataTableExportRequest): Promise<DataTableExportResponse> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/datatable/${tableKey}/export`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {}),
      },
      body: JSON.stringify(request),
    })
    if (!res.ok) {
      const error = await res.json().catch(() => null)
      throw new Error(problemMessage(error, 'Failed to create export'))
    }
    return res.json()
  },
}