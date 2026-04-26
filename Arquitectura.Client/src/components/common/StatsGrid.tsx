import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card'

interface StatsGridProps {
  stats: Array<{
    title: string
    value: string | number
    icon: React.ElementType
  }>
  isDark: boolean
}

export function StatsGrid({ stats, isDark }: StatsGridProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      {stats.map((stat, index) => (
        <Card 
          key={index}
          className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95 border-slate-300/50'}
        >
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className={`text-sm font-medium ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>
              {stat.title}
            </CardTitle>
            <stat.icon className={`h-4 w-4 ${isDark ? 'text-blue-400' : 'text-blue-600'}`} />
          </CardHeader>
          <CardContent>
            <div className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-slate-950'}`}>
              {stat.value}
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}