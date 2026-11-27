import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { useCreateForecast, useUpdateForecast } from '@/services/weatherApi'
import type { WeatherForecast, CreateForecastRequest } from '@/services/weatherApi'
import { Loader2, Calendar, Thermometer, Cloud } from 'lucide-react'
import { toast } from 'sonner'
import { useTheme } from 'next-themes'

const forecastSchema = z.object({
  date: z.string().min(1, 'Date is required'),
  temperatureC: z.number().min(-100).max(100),
  summary: z.string().max(500).default(''),
})

type ForecastFormData = z.infer<typeof forecastSchema>

interface WeatherFormProps {
  forecast?: WeatherForecast | null
  onCancel: () => void
  onSuccess: () => void
}

export function WeatherForm({ forecast, onCancel, onSuccess }: WeatherFormProps) {
  const { resolvedTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'
  const isEditing = !!forecast
  const createMutation = useCreateForecast()
  const updateMutation = useUpdateForecast()
  const isPending = createMutation.isPending || updateMutation.isPending

  const form = useForm<ForecastFormData>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(forecastSchema) as any,
    defaultValues: {
      date: forecast?.date || new Date().toISOString().split('T')[0],
      temperatureC: forecast?.temperatureC ?? 20,
      summary: forecast?.summary ?? '',
    },
  })

  const onSubmit = form.handleSubmit(async (data) => {
    const requestData: CreateForecastRequest = {
      date: data.date,
      temperatureC: data.temperatureC,
      summary: data.summary,
    }

    if (isEditing && forecast) {
      updateMutation.mutate(
        { id: forecast.id, data: requestData },
        {
          onSuccess: () => {
            toast.success('Forecast updated successfully')
            form.reset()
            onSuccess()
          },
          onError: (error: Error) => {
            toast.error('Failed to update forecast', {
              description: error.message,
            })
          },
        }
      )
    } else {
      createMutation.mutate(requestData, {
        onSuccess: () => {
          toast.success('Forecast created successfully')
          form.reset()
          onSuccess()
        },
        onError: (error: Error) => {
          toast.error('Failed to create forecast', {
            description: error.message,
          })
        },
      })
    }
  })

  return (
    <Form {...form}>
      <form onSubmit={onSubmit} className="space-y-5">
        <FormField
          control={form.control}
          name="date"
          render={({ field }) => (
            <FormItem>
              <FormLabel className={isDark ? 'text-slate-300' : 'text-slate-700'}>
                <Calendar className="h-4 w-4 mr-2 inline" />
                Date
              </FormLabel>
              <FormControl>
                <Input 
                  type="date" 
                  {...field} 
                  className={`${
                    isDark 
                      ? 'bg-slate-800 border-slate-600 text-white placeholder:text-slate-500 focus:border-blue-500' 
                      : 'bg-slate-50 border-slate-300 text-slate-900 placeholder:text-slate-400 focus:border-blue-500'
                  }`}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="temperatureC"
          render={({ field }) => (
            <FormItem>
              <FormLabel className={isDark ? 'text-slate-300' : 'text-slate-700'}>
                <Thermometer className="h-4 w-4 mr-2 inline" />
                Temperature (°C)
              </FormLabel>
              <FormControl>
                <Input
                  type="number"
                  value={field.value ?? ''}
                  onChange={(e) => field.onChange(e.target.value === '' ? 0 : Number(e.target.value))}
                  className={`${
                    isDark 
                      ? 'bg-slate-800 border-slate-600 text-white placeholder:text-slate-500 focus:border-blue-500' 
                      : 'bg-slate-50 border-slate-300 text-slate-900 placeholder:text-slate-400 focus:border-blue-500'
                  }`}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="summary"
          render={({ field }) => (
            <FormItem>
              <FormLabel className={isDark ? 'text-slate-300' : 'text-slate-700'}>
                <Cloud className="h-4 w-4 mr-2 inline" />
                Summary
              </FormLabel>
              <FormControl>
                <Input
                  type="text"
                  placeholder="e.g., Sunny, Cloudy, Rainy..."
                  {...field}
                  className={`${
                    isDark 
                      ? 'bg-slate-800 border-slate-600 text-white placeholder:text-slate-500 focus:border-blue-500' 
                      : 'bg-slate-50 border-slate-300 text-slate-900 placeholder:text-slate-400 focus:border-blue-500'
                  }`}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex gap-3 pt-3">
          <Button 
            type="submit" 
            disabled={isPending} 
            className="flex-1 bg-blue-600 hover:bg-blue-700 text-white border-0"
          >
            {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEditing ? 'Update Forecast' : 'Create Forecast'}
          </Button>
          <Button 
            type="button" 
            variant="outline" 
            onClick={onCancel} 
            disabled={isPending}
            className={isDark ? 'border-slate-600 hover:bg-slate-800 text-white' : ''}
          >
            Cancel
          </Button>
        </div>
      </form>
    </Form>
  )
}