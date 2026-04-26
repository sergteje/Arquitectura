import { useState } from 'react'
import { useWeatherForecasts, useDeleteForecast } from '@/services/weatherApi'
import type { WeatherForecast } from '@/services/weatherApi'
import { Thermometer, Cloud, Wind, Sun } from 'lucide-react'
import { toast } from 'sonner'
import { useTheme } from 'next-themes'
import { DataTable, StatsGrid, DeleteDialog, PageHeader } from '@/components/common'
import { useWeatherColumns } from '@/components/weather/useWeatherColumns'

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
  
  const columns = useWeatherColumns(isDark)

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

      {/* Data Table using new generic component */}
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