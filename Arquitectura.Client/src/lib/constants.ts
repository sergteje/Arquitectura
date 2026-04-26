export const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:8080'

export const APP_NAME = 'Arquitectura'
export const APP_TAGLINE = 'Modern .NET + React'

export const COLORS = {
  primary: {
    bg: 'bg-blue-600',
    hover: 'hover:bg-blue-700',
    light: 'bg-blue-500/20',
  },
  danger: {
    text: 'text-red-500',
    hover: 'hover:text-red-500',
    bg: 'bg-red-500/10',
  },
} as const

// Weather icons mapping
export const WEATHER_ICONS: Record<string, string> = {
  'Freezing': '❄️',
  'Chilly': '🥶',
  'Cool': '😌',
  'Mild': '😊',
  'Warm': '😎',
  'Hot': '🔥',
  'Sunny': '☀️',
  'Cloudy': '☁️',
  'Rainy': '🌧️',
  'Stormy': '⛈️',
}