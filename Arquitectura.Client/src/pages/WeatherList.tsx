import { useState } from 'react'
import { useWeatherForecasts, useDeleteForecast } from '@/services/weatherApi'
import type { WeatherForecast } from '@/services/weatherApi'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Skeleton } from '@/components/ui/skeleton'
import { Plus, Trash2, Loader2, Thermometer, Cloud, Wind, Sun, Edit2 } from 'lucide-react'
import { toast } from 'sonner'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { useTheme } from 'next-themes'
import { WEATHER_ICONS, COLORS } from '@/lib/constants'

interface WeatherListProps {
  onCreate: () => void
  onEdit: (forecast: WeatherForecast) => void
}

export function WeatherList({ onCreate, onEdit }: WeatherListProps) {
  const [page, setPage] = useState(1)
  const [deleteId, setDeleteId] = useState<number | null>(null)
  const { data, isLoading, error } = useWeatherForecasts(page, 20)
  const deleteMutation = useDeleteForecast()
  const { theme } = useTheme()
  const isDark = theme === 'dark'

  const handleDelete = (id: number) => setDeleteId(id)

  const confirmDelete = () => {
    if (deleteId !== null) {
      deleteMutation.mutate(deleteId, {
        onSuccess: () => toast.success('Forecast deleted'),
        onError: () => toast.error('Failed to delete'),
        onSettled: () => setDeleteId(null),
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
        <Button onClick={onCreate} size="lg" className={`${COLORS.primary.bg} ${COLORS.primary.hover} text-white border-0 shadow-lg shadow-blue-500/30`}>
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
      {isLoading && (
        <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95'}>
          <CardContent className="pt-6">
            <div className="space-y-4">
              {[...Array(5)].map((_, i) => (
                <div key={i} className="flex items-center gap-4">
                  <Skeleton className="h-4 w-12" />
                  <Skeleton className="h-4 w-24" />
                  <Skeleton className="h-4 w-20" />
                  <Skeleton className="h-8 w-20 ml-auto" />
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Error */}
      {error && (
        <Card className="border-red-500/50 bg-red-500/10">
          <CardHeader>
            <CardTitle className="text-red-500">Error</CardTitle>
            <CardDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
              Failed to load weather forecasts.
            </CardDescription>
          </CardHeader>
          <CardFooter>
            <Button variant="outline" onClick={() => window.location.reload()}>Retry</Button>
          </CardFooter>
        </Card>
      )}

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
              <div className="flex flex-col items-center justify-center py-16">
                <div className={`p-4 rounded-full ${isDark ? 'bg-slate-800' : 'bg-slate-100'} mb-4`}>
                  <Cloud className={`h-8 w-8 ${isDark ? 'text-slate-500' : 'text-slate-400'}`} />
                </div>
                <p className={`text-lg font-medium ${isDark ? 'text-slate-400' : 'text-slate-600'}`}>
                  No forecasts found
                </p>
                <p className={`text-sm ${isDark ? 'text-slate-500' : 'text-slate-500'} mb-4`}>
                  Get started by creating your first forecast
                </p>
                <Button variant="outline" size="sm" onClick={onCreate}>
                  <Plus className="mr-2 h-4 w-4" />
                  Create your first forecast
                </Button>
              </div>
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
                    <TableRow key={forecast.id} className={`${isDark ? 'border-white/5' : 'border-slate-100'}`}>
                      <TableCell>
                        <span className={`inline-flex px-2 py-1 rounded-md text-xs font-medium ${
                          isDark ? 'bg-slate-800 text-slate-300' : 'bg-slate-100 text-slate-700'
                        }`}>
                          #{forecast.id}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span className={isDark ? 'bg-blue-500/20 text-blue-300 px-2 py-1 rounded-md' : 'bg-blue-50 text-blue-700 px-2 py-1 rounded-md'}>
                          {forecast.date}
                        </span>
                      </TableCell>
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
                      <TableCell>
                        {forecast.summary ? (
                          <div className="flex items-center gap-2">
                            <span className="text-lg">{WEATHER_ICONS[forecast.summary] || '🌤️'}</span>
                            <span className={isDark ? 'text-slate-300' : 'text-slate-700'}>{forecast.summary}</span>
                          </div>
                        ) : (
                          <span className={isDark ? 'text-slate-500 italic' : 'text-slate-400 italic'}>No summary</span>
                        )}
                      </TableCell>
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
                            onClick={() => handleDelete(forecast.id)}
                            disabled={deleteMutation.isPending}
                            title="Delete"
                          >
                            {deleteMutation.isPending ? <Loader2 className="h-3 w-3 animate-spin" /> : <Trash2 className="h-3 w-3" />}
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>

          {/* Pagination */}
          {data.totalPages > 1 && (
            <CardFooter className={`flex items-center justify-between ${isDark ? 'border-white/10' : 'border-slate-200'} py-4`}>
              <Button variant="ghost" size="sm" onClick={() => setPage(p => Math.max(1, p - 1))} disabled={!data.hasPreviousPage}>
                Previous
              </Button>
              <span className={isDark ? 'text-slate-400' : 'text-slate-600'}>Page {data.page} of {data.totalPages}</span>
              <Button variant="ghost" size="sm" onClick={() => setPage(p => p + 1)} disabled={!data.hasNextPage}>
                Next
              </Button>
            </CardFooter>
          )}
        </Card>
      )}

      {/* Delete Dialog */}
      <AlertDialog open={deleteId !== null} onOpenChange={(open) => !open && setDeleteId(null)}>
        <AlertDialogContent className={isDark ? 'bg-slate-900 border-slate-700' : 'bg-white'}>
          <AlertDialogHeader>
            <AlertDialogTitle className={isDark ? 'text-white' : 'text-slate-950'}>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel className={isDark ? 'bg-slate-800 text-white hover:bg-slate-700' : ''}>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={confirmDelete} className="bg-red-600 hover:bg-red-700 text-white">
              {deleteMutation.isPending ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  )
}

// Stat Card component
function StatCard({ title, value, icon: Icon, isDark }: { title: string; value: string | number; icon: React.ElementType; isDark: boolean }) {
  return (
    <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95 border-slate-300/50'}>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className={`text-sm font-medium ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>{title}</CardTitle>
        <Icon className={`h-4 w-4 ${isDark ? 'text-blue-400' : 'text-blue-600'}`} />
      </CardHeader>
      <CardContent>
        <div className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-slate-950'}`}>{value}</div>
      </CardContent>
    </Card>
  )
}