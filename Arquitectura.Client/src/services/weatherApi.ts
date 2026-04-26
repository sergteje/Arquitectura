import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuthStore } from '@/stores/authStore'

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:8081'

// Types
export interface WeatherForecast {
  id: number
  date: string
  temperatureC: number
  temperatureF: number
  summary: string
}

export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface CreateForecastRequest {
  date: string
  temperatureC: number
  summary: string
}

// API functions
export const weatherApi = {
  getAll: async (page = 1, pageSize = 20): Promise<PaginatedResponse<WeatherForecast>> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/weatherforecast?page=${page}&pageSize=${pageSize}`, {
      headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {},
    })
    if (res.status === 204) {
      return {
        items: [],
        page,
        pageSize,
        totalCount: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
      }
    }
    if (!res.ok) throw new Error('Failed to fetch forecasts')
    return res.json()
  },

  getById: async (id: number): Promise<WeatherForecast> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/weatherforecast/${id}`, {
      headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {},
    })
    if (!res.ok) throw new Error('Failed to fetch forecast')
    return res.json()
  },

  create: async (data: CreateForecastRequest): Promise<WeatherForecast> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/weatherforecast`, {
      method: 'POST',
      headers: { 
        'Content-Type': 'application/json',
        ...(accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {})
      },
      body: JSON.stringify(data),
    })
    if (!res.ok) throw new Error('Failed to create forecast')
    return res.json()
  },

  update: async (id: number, data: CreateForecastRequest): Promise<WeatherForecast> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/weatherforecast/${id}`, {
      method: 'PUT',
      headers: { 
        'Content-Type': 'application/json',
        ...(accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {})
      },
      body: JSON.stringify(data),
    })
    if (!res.ok) throw new Error('Failed to update forecast')
    return res.json()
  },

  delete: async (id: number): Promise<void> => {
    const accessToken = useAuthStore.getState().accessToken
    const res = await fetch(`${API_BASE}/api/weatherforecast/${id}`, {
      method: 'DELETE',
      headers: accessToken ? { 'Authorization': `Bearer ${accessToken}` } : {},
    })
    if (!res.ok) throw new Error('Failed to delete forecast')
  },
}

// Hooks
export function useWeatherForecasts(page = 1, pageSize = 20) {
  return useQuery({
    queryKey: ['weatherforecasts', page, pageSize],
    queryFn: () => weatherApi.getAll(page, pageSize),
  })
}

export function useWeatherForecast(id: number) {
  return useQuery({
    queryKey: ['weatherforecast', id],
    queryFn: () => weatherApi.getById(id),
    enabled: !!id,
  })
}

export function useCreateForecast() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateForecastRequest) => weatherApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['weatherforecasts'] })
      queryClient.refetchQueries({ queryKey: ['weatherforecasts'] })
    },
  })
}

export function useUpdateForecast() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: CreateForecastRequest }) =>
      weatherApi.update(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['weatherforecasts'] })
      queryClient.invalidateQueries({ queryKey: ['weatherforecast', id] })
    },
  })
}

export function useDeleteForecast() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => weatherApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['weatherforecasts'] })
    },
  })
}