import { useState } from 'react'
import { useWeatherForecasts, useDeleteForecast } from '@/services/weatherApi'
import type { WeatherForecast } from '@/services/weatherApi'
import { Thermometer, Cloud, Wind, Sun } from 'lucide-react'
import { toast } from 'sonner'
import { useTheme } from 'next-themes'
import { DataTable, StatsGrid, DeleteDialog, PageHeader, DataTableToolbar } from '@/components/common'
import { useDataTableConfig } from '@/hooks'
import type { Column } from '@/components/common'

interface WeatherListProps {
  onCreate: () => void
  onEdit: (forecast: WeatherForecast) => void
}

export function WeatherList({ onCreate, onEdit }: WeatherListProps) {
  const [page, setPage] = useState(1)
  const [deleteId, setDeleteId] = useState<number | null>(null)
  const { data, isLoading, error } = useWeatherForecasts(page, 20)
  const deleteMutation = useDeleteForecast()
  const { resolvedTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  // DataTable Config - obtiene columnas del backend
  const {
    config: tableColumns,
    visibleColumnKeys,
    toggleColumn,
    saveConfig,
    resetConfig,
    hasUnsavedChanges,
    isSaving,
    isLoading: isConfigLoading,
  } = useDataTableConfig({ tableKey: 'WeatherForecasts' })

  // Mapear columnas del backend con los renders del frontend
  // Normalizar PascalCase (backend) a camelCase (frontend)
  const getWeatherRender = (key: string) => {
    const renders: Record<string, (item: WeatherForecast, isDark: boolean) => React.ReactNode> = {
      id: (item) => <span className={isDark ? 'text-slate-400' : 'text-slate-600'}>#{item.id}</span>,
      date: (item) => (
        <span className={isDark ? 'bg-blue-500/20 text-blue-300 px-2 py-1 rounded-md' : 'bg-blue-50 text-blue-700 px-2 py-1 rounded-md'}>
          {item.date}
        </span>
      ),
      temperatureC: (item) => {
        const temp = item.temperatureC
        const getIcon = () => {
          if (temp >= 30) return '🔥'
          if (temp >= 20) return '☀️'
          if (temp >= 10) return '😊'
          if (temp >= 0) return '😌'
          if (temp >= -10) return '🥶'
          return '❄️'
        }
        return (
          <>
            <span className="mr-1">{getIcon()}</span>
            <span className={`font-bold text-lg ${
              temp > 20 ? isDark ? 'text-red-400' : 'text-red-600'
                : temp < 5 ? isDark ? 'text-blue-400' : 'text-blue-600'
                : isDark ? 'text-yellow-400' : 'text-yellow-600'
            }`}>
              {temp}°C
            </span>
            <span className={isDark ? 'text-slate-500' : 'text-slate-400'}> /{item.temperatureF}°F</span>
          </>
        )
      },
      summary: (item) => item.summary ? (
        <span className={isDark ? 'text-slate-300' : 'text-slate-700'}>{item.summary}</span>
      ) : (
        <span className={isDark ? 'text-slate-500 italic' : 'text-slate-400 italic'}>Sin resumen</span>
      ),
    }
    return renders[key]
  }

  // Construir columnas filtradas por visibilidad del backend
  // Convertir PascalCase a camelCase para que matcheen con WeatherForecast
  const columns: Column<WeatherForecast>[] = visibleColumnKeys
    .map(key => {
      const colConfig = tableColumns.find(c => c.key === key)
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1)
      const customRender = getWeatherRender(camelKey)
      return {
        key: camelKey,
        header: colConfig?.label || key,
        render: customRender ? ((item: WeatherForecast) => customRender(item, isDark)) : undefined,
      }
    })

  const handleDelete = (id: number) => setDeleteId(id)

  const confirmDelete = () => {
    if (deleteId !== null) {
      deleteMutation.mutate(deleteId, {
        onSuccess: () => {
          toast.success('Forecast deleted')
          setDeleteId(null)
        },
        onError: () => {
          toast.error('Failed to delete')
          setDeleteId(null)
        },
      })
    }
  }

  // Stats calculation
  const stats = data?.items.length
    ? [
        { title: 'Total', value: data.totalCount, icon: Cloud },
        { title: 'Avg', value: `${Math.round(data.items.reduce((acc, f) => acc + f.temperatureC, 0) / data.items.length)}°C`, icon: Thermometer },
        { title: 'Warmest', value: `${Math.max(...data.items.map(f => f.temperatureC))}°C`, icon: Sun },
        { title: 'Coolest', value: `${Math.min(...data.items.map(f => f.temperatureC))}°C`, icon: Wind },
      ]
    : []

  return (
    <div className="space-y-6">
      <PageHeader
        title="Weather Forecasts"
        description="Manage your weather data"
        isDark={isDark}
      />

      {/* Stats */}
      {!isLoading && !error && data && data.items.length > 0 && (
        <StatsGrid stats={stats} isDark={isDark} />
      )}

      {/* Data Table */}
      <DataTable
        data={data ?? null}
        isLoading={isLoading}
        error={error ?? null}
        columns={columns}
        icon={Thermometer}
        title="Forecast List"
        description={data ? `${data.totalCount} forecast${data.totalCount !== 1 ? 's' : ''} in total` : undefined}
        emptyTitle="No forecasts found"
        emptyDescription="Get started by creating your first forecast."
        emptyActionLabel="Create first forecast"
        onCreate={onCreate}
        onEdit={onEdit}
        onDelete={handleDelete}
        getId={(item) => item.id}
        onPageChange={setPage}
      />

      {/* DataTable Config Toolbar */}
      {!isConfigLoading && tableColumns.length > 0 && (
        <div className="flex justify-end">
          <DataTableToolbar
            columns={tableColumns}
            visibleColumns={visibleColumnKeys}
            onToggleColumn={toggleColumn}
            onSaveConfig={saveConfig}
            onResetConfig={resetConfig}
            hasUnsavedChanges={hasUnsavedChanges}
            isSaving={isSaving}
            isDark={isDark}
          />
        </div>
      )}

      {/* Delete Dialog */}
      <DeleteDialog
        isOpen={deleteId !== null}
        isDark={isDark}
        isPending={deleteMutation.isPending}
        onOpenChange={(open) => !open && setDeleteId(null)}
        onConfirm={confirmDelete}
      />
    </div>
  )
}