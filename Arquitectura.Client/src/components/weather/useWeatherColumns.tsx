import type { WeatherForecast } from '@/services/weatherApi'
import { useColumns, type ColumnConfig } from '@/components/common'
import { WEATHER_ICONS } from '@/lib/constants'

export function useWeatherColumns(isDark: boolean) {
  const config: ColumnConfig<WeatherForecast>[] = [
    { 
      key: 'date',
      header: 'Date',
      render: (item) => (
        <span className={isDark ? 'bg-blue-500/20 text-blue-300 px-2 py-1 rounded-md' : 'bg-blue-50 text-blue-700 px-2 py-1 rounded-md'}>
          {item.date}
        </span>
      )
    },
    { 
      key: 'temperatureC',
      header: 'Temp',
      render: (item) => {
        const getIcon = (temp: number) => {
          if (temp >= 30) return '🔥'
          if (temp >= 20) return '☀️'
          if (temp >= 10) return '😊'
          if (temp >= 0) return '😌'
          if (temp >= -10) return '🥶'
          return '❄️'
        }
        return (
          <>
            <span className="mr-1">{getIcon(item.temperatureC)}</span>
            <span className={`font-bold text-lg ${
              item.temperatureC > 20 
                ? isDark ? 'text-red-400' : 'text-red-600'
                : item.temperatureC < 5 
                  ? isDark ? 'text-blue-400' : 'text-blue-600'
                  : isDark ? 'text-yellow-400' : 'text-yellow-600'
            }`}>
              {item.temperatureC}°C
            </span>
            <span className={isDark ? 'text-slate-500' : 'text-slate-400'}> /{item.temperatureF}°F</span>
          </>
        )
      }
    },
    { 
      key: 'summary',
      header: 'Summary',
      render: (item) => item.summary ? (
        <>
          <span className="mr-1 text-lg">{WEATHER_ICONS[item.summary] || '🌤️'}</span>
          <span className={isDark ? 'text-slate-300' : 'text-slate-700'}>{item.summary}</span>
        </>
      ) : (
        <span className={isDark ? 'text-slate-500 italic' : 'text-slate-400 italic'}>No summary</span>
      )
    },
  ]

  return useColumns(config, isDark)
}