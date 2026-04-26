import React from 'react'
import { useTheme } from 'next-themes'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from '@/components/ui/card'
import { Table, TableBody, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Plus } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'
import { EmptyState, LoadingState, ErrorState } from './index'
import { DataRow, type Column } from './index'

export interface DataTableProps<T> {
  // Data
  data?: {
    items: T[]
    page: number
    pageSize: number
    totalCount: number
    totalPages: number
    hasNextPage: boolean
    hasPreviousPage: boolean
  } | null
  isLoading?: boolean
  error?: Error | null

  // Columns
  columns: Column<T>[]

  // Icon (optional)
  icon?: LucideIcon

  // Actions (optional)
  onCreate?: () => void
  onEdit?: (item: T) => void
  onDelete?: (id: number) => void

  // UI
  title: string
  description?: string
  createLabel?: string
  emptyTitle?: string
  emptyDescription?: string
  emptyActionLabel?: string

  // Get ID
  getId: (item: T) => number

  // Pagination callbacks
  onPageChange?: (page: number) => void
}

export function DataTable<T>({
  data,
  isLoading,
  error,
  columns,
  icon,
  onCreate,
  onEdit,
  onDelete,
  title,
  description,
  createLabel = 'Create',
  emptyTitle = 'No data found',
  emptyDescription = 'Get started by creating your first item.',
  emptyActionLabel = 'Create',
  getId,
  onPageChange,
}: DataTableProps<T>) {
  const { resolvedTheme } = useTheme()
  const isDark = resolvedTheme === 'dark'

  const handlePrevPage = () => {
    if (onPageChange && data) onPageChange(data.page - 1)
  }

  const handleNextPage = () => {
    if (onPageChange && data) onPageChange(data.page + 1)
  }

  // Loading
  if (isLoading) {
    return (
      <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95'}>
        <CardHeader className="pb-3">
          <CardTitle className={isDark ? 'text-white' : 'text-slate-950'}>{title}</CardTitle>
        </CardHeader>
        <LoadingState isDark={isDark} />
      </Card>
    )
  }

  // Error
  if (error) {
    return (
      <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95'}>
        <CardHeader className="pb-3">
          <CardTitle className={isDark ? 'text-white' : 'text-slate-950'}>{title}</CardTitle>
        </CardHeader>
        <CardContent>
          <ErrorState isDark={isDark} />
        </CardContent>
      </Card>
    )
  }

  // No data
  if (!data || data.items.length === 0) {
    return (
      <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95 border-slate-300/50'}>
        <CardHeader className="pb-3 flex flex-row items-center justify-between">
          <div>
            <CardTitle className={isDark ? 'text-white' : 'text-slate-950'}>{title}</CardTitle>
            {description && (
              <CardDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
                {description}
              </CardDescription>
            )}
          </div>
          {onCreate && (
            <Button 
              onClick={onCreate} 
              size="sm"
              className={isDark ? 'bg-blue-600 hover:bg-blue-700 text-white' : ''}
            >
              <Plus className="h-4 w-4 mr-2" />
              {createLabel}
            </Button>
          )}
        </CardHeader>
        <CardContent>
          <EmptyState
            isDark={isDark}
            title={emptyTitle}
            description={emptyDescription}
            actionLabel={emptyActionLabel}
            onAction={onCreate}
          />
        </CardContent>
      </Card>
    )
  }

  // With data
  return (
    <Card className={isDark ? 'bg-white/5 border-white/10' : 'bg-white/95 border-slate-300/50'}>
      {/* Header */}
      <CardHeader className="pb-3 flex flex-row items-center justify-between">
        <div className="flex items-center gap-3">
          {icon && (
            <div className={`p-2 rounded-lg ${isDark ? 'bg-blue-500/20' : 'bg-blue-100'}`}>
              {React.createElement(icon, { className: `h-5 w-5 ${isDark ? 'text-blue-400' : 'text-blue-600'}` })}
            </div>
          )}
          <div>
            <CardTitle className={isDark ? 'text-white' : 'text-slate-950'}>{title}</CardTitle>
            {description && (
              <CardDescription className={isDark ? 'text-slate-400' : 'text-slate-700'}>
                {description}
              </CardDescription>
            )}
          </div>
        </div>
        {onCreate && (
          <Button 
            onClick={onCreate} 
            size="sm"
            className={isDark ? 'bg-blue-600 hover:bg-blue-700 text-white' : ''}
          >
            <Plus className="h-4 w-4 mr-2" />
            {createLabel}
          </Button>
        )}
      </CardHeader>

      {/* Table */}
      <CardContent className="p-0">
        <Table>
          <TableHeader>
            <TableRow className={`hover:bg-transparent ${isDark ? 'border-white/10' : 'border-slate-200'}`}>
              {columns.map((col) => (
                <TableHead 
                  key={col.key}
                  className={isDark ? 'text-slate-400' : 'text-slate-700'}
                >
                  {col.header}
                </TableHead>
              ))}
              {(onEdit || onDelete) && (
                <TableHead className={`text-right ${isDark ? 'text-slate-400' : 'text-slate-700'}`}>
                  Actions
                </TableHead>
              )}
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.map((item) => (
              <DataRow
                key={getId(item)}
                item={item}
                columns={columns}
                isDark={isDark}
                onEdit={onEdit ? () => onEdit(item) : undefined}
                onDelete={onDelete ? (id) => onDelete(id as number) : undefined}
                getId={getId}
              />
            ))}
          </TableBody>
        </Table>
      </CardContent>

      {/* Pagination */}
      {data.totalPages > 1 && onPageChange && (
        <CardFooter className={`flex items-center justify-between ${isDark ? 'border-white/10' : 'border-slate-200'} py-4`}>
          <Button 
            variant="ghost" 
            size="sm" 
            onClick={handlePrevPage}
            disabled={!data.hasPreviousPage}
          >
            Previous
          </Button>
          <span className={isDark ? 'text-slate-400' : 'text-slate-600'}>
            {data.page} / {data.totalPages}
          </span>
          <Button 
            variant="ghost" 
            size="sm" 
            onClick={handleNextPage}
            disabled={!data.hasNextPage}
          >
            Next
          </Button>
        </CardFooter>
      )}
    </Card>
  )
}