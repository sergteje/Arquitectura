import { Button } from '@/components/ui/button'
import { CardFooter } from '@/components/ui/card'
import { ChevronLeft, ChevronRight } from 'lucide-react'

interface WeatherPaginationProps {
  isDark: boolean
  page: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  onPageChange: (page: number) => void
}

export function WeatherPagination({ isDark, page, totalPages, hasPreviousPage, hasNextPage, onPageChange }: WeatherPaginationProps) {
  return (
    <CardFooter className={`flex items-center justify-between ${isDark ? 'border-white/10' : 'border-slate-200'} py-4`}>
      <Button 
        variant="ghost" 
        size="sm" 
        onClick={() => onPageChange(page - 1)}
        disabled={!hasPreviousPage}
      >
        <ChevronLeft className="h-4 w-4 mr-1" />
        Previous
      </Button>
      <span className={isDark ? 'text-slate-400' : 'text-slate-600'}>
        {page} / {totalPages}
      </span>
      <Button 
        variant="ghost" 
        size="sm" 
        onClick={() => onPageChange(page + 1)}
        disabled={!hasNextPage}
      >
        Next
        <ChevronRight className="h-4 w-4 ml-1" />
      </Button>
    </CardFooter>
  )
}