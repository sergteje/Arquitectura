import React from 'react'
import type { Column } from '@/components/common'

export interface ColumnConfig<T> {
  key: keyof T
  header: string
  render?: (item: T, isDark: boolean) => React.ReactNode
}

export function useColumns<T>(config: ColumnConfig<T>[], isDark: boolean): Column<T>[] {
  return config.map((col) => ({
    key: String(col.key),
    header: col.header,
    render: col.render 
      ? (item) => col.render!(item, isDark)
      : undefined
  }))
}