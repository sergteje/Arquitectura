import { useState } from 'react'
import { useWeatherForecasts, useDeleteForecast } from '@/services/weatherApi'
import type { WeatherForecast } from '@/services/weatherApi'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Table, TableBody, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Plus, Thermometer, Cloud, Wind, Sun } from 'lucide-react'
import { toast } from 'sonner'
import { useTheme } from 'next-themes'
import { StatCard, WeatherRow, EmptyState, LoadingState, ErrorState, WeatherPagination, DeleteDialog } from '@/components/weather'
import { COLORS } from '@/lib/constants'

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

  // Stats
  const avgTemp = data?.items.length 
    ? Math.round(data.items.reduce((acc, f) => acc + f.temperatureC, 0) / data.items.length)
    : 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h2 className={`text-3xl font-bold tracking-tight ${isDark ? 'text-white' : 'text-slate-950'}`}>
            Weather Forecasts
          </h2>
          <p className={`mt-1 ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>
            Manage your weather data
          </p>
        </div>
        <Button 
          onClick={onCreate} 
          size="lg" 
          className={`${COLORS.primary.bg} ${COLORS.primary.hover} text-white border-0 shadow-lg shadow-blue-500/30`}
        >
          <Plus className="mr-2 h-4 w-4" />
          New Forecast
        </Button>
      </div>

      {/* Stats Dashboard */}
      {!isLoading && !error && data && data.items.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatCard title="Total" value={data.totalCount} icon={Cloud} isDark={isDark} />
          <StatCard title="Avg" value={`${avgTemp}°C`} icon={Thermometer} isDark={isDark} />
          <StatCard title="Warmest" value={`${Math.max(...data.items.map(f => f.temperatureC))}°C`} icon={Sun} isDark={isDark} />
          <StatCard title="Coolest" value={`${Math.min(...data.items.map(f => f.temperatureC))}°C`} icon={Wind} isDark={isDark} />
        </div>
      )}

      {/* Loading */}
      {isLoading && <LoadingState isDark={isDark} />}

      {/* Error */}
      {error && <ErrorState isDark={isDark} />}

      {/* Data Table */}
      {!isLoading && !error && data && (
        <Card className={`${isDark ? 'bg-white/5 border-white/10' : 'bg-white/95 border-slate-300/50'} overflow-hidden`}>
          <CardHeader className="pb-3">
            <div className="flex items-center gap-2">
              <div className={`p-2 rounded-lg ${isDark ? 'bg-blue-500/20' : 'bg-blue-100'}`}>
                <Thermometer className={`h-5 w-5 ${isDark ? 'text-blue-400' : 'text-blue-600'}`} />
              </div>
              <div>
                <CardTitle className={`text-lg ${isDark ? 'text-white' : 'text-slate-950'}`}>
                  Forecast List
                </CardTitle>
                <CardDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
                  {data.totalCount} forecast{data.totalCount !== 1 ? 's' : ''} in total
                </CardDescription>
              </div>
            </div>
          </CardHeader>

          <CardContent className="p-0">
            {data.items.length === 0 ? (
              <EmptyState isDark={isDark} onCreate={onCreate} />
            ) : (
              <Table>
                <TableHeader>
                  <TableRow className={`hover:bg-transparent ${isDark ? 'border-white/10' : 'border-slate-200'}`}>
                    <TableHead className={isDark ? 'text-slate-400' : 'text-slate-700'}>ID</TableHead>
                    <TableHead className={isDark ? 'text-slate-400' : 'text-slate-700'}>Date</TableHead>
                    <TableHead className={isDark ? 'text-slate-400' : 'text-slate-700'}>Temp</TableHead>
                    <TableHead className={isDark ? 'text-slate-400' : 'text-slate-700'}>Summary</TableHead>
                    <TableHead className={`text-right ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.items.map((forecast) => (
                    <WeatherRow
                      key={forecast.id}
                      forecast={forecast}
                      isDark={isDark}
                      isDeleting={deleteMutation.isPending}
                      onEdit={onEdit}
                      onDelete={handleDelete}
                    />
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>

          {/* Pagination */}
          {data.totalPages > 1 && (
            <WeatherPagination
              isDark={isDark}
              page={data.page}
              totalPages={data.totalPages}
              hasPreviousPage={data.hasPreviousPage}
              hasNextPage={data.hasNextPage}
              onPageChange={setPage}
            />
          )}
        </Card>
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