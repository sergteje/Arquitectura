import React from 'react'

export interface Column<T> {
  key: string
  header: string
  render?: (item: T, isDark: boolean) => React.ReactNode
}