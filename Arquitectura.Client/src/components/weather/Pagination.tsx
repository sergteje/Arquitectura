import { Button } from '@/components/ui/button'
import { CardFooter } from '@/components/ui/card'

interface PaginationProps {
  isDark: boolean
  page: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  onPageChange: (page: number) => void
}

export function WeatherPagination({ isDark, page, totalPages, hasPreviousPage, hasNextPage, onPageChange }: PaginationProps) {
  return (
    <CardFooter className={`flex items-center justify-between ${isDark ? 'border-white/10' : 'border-slate-200'} py-4`}>
      <Button 
        variant="ghost" 
        size="sm" 
        onClick={() => onPageChange(page - 1)}
        disabled={!hasPreviousPage}
      >
        Previous
      </Button>
      <span className={isDark ? 'text-slate-400' : 'text-slate-600'}>
        Page {page} of {totalPages}
      </span>
      <Button 
        variant="ghost" 
        size="sm" 
        onClick={() => onPageChange(page + 1)}
        disabled={!hasNextPage}
      >
        Next
      </Button>
    </CardFooter>
  )
}