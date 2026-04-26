import { Button } from '@/components/ui/button'
import { CardFooter } from '@/components/ui/card'

interface PaginationProps {
  isDark: boolean
  page: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  onPageChange: (page: number) => void
  previousLabel?: string
  nextLabel?: string
}

export function Pagination({ 
  isDark, 
  page, 
  totalPages, 
  hasPreviousPage, 
  hasNextPage, 
  onPageChange,
  previousLabel = 'Previous',
  nextLabel = 'Next'
}: PaginationProps) {
  return (
    <CardFooter className={`flex items-center justify-between ${isDark ? 'border-white/10' : 'border-slate-200'} py-4`}>
      <Button 
        variant="ghost" 
        size="sm" 
        onClick={() => onPageChange(page - 1)}
        disabled={!hasPreviousPage}
      >
        {previousLabel}
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
        {nextLabel}
      </Button>
    </CardFooter>
  )
}