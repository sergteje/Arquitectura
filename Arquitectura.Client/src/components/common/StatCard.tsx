import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

interface StatCardProps {
  title: string
  value: string | number
  icon: React.ElementType
  isDark: boolean
}

export function StatCard({ title, value, icon: Icon, isDark }: StatCardProps) {
  return (
    <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95 border-slate-300/50'}>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className={`text-sm font-medium ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>
          {title}
        </CardTitle>
        <Icon className={`h-4 w-4 ${isDark ? 'text-blue-400' : 'text-blue-600'}`} />
      </CardHeader>
      <CardContent>
        <div className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-slate-950'}`}>
          {value}
        </div>
      </CardContent>
    </Card>
  )
}