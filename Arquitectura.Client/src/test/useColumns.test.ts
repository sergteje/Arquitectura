import { describe, it, expect } from 'vitest'
import { useColumns } from '../components/common/useColumns'
import React from 'react'

describe('useColumns', () => {
  it('should transform column config to columns', () => {
    const config = [
      { key: 'name' as const, header: 'Name' },
      { key: 'age' as const, header: 'Age' },
    ]
    
    const columns = useColumns(config, false)
    
    expect(columns).toHaveLength(2)
    expect(columns[0].key).toBe('name')
    expect(columns[0].header).toBe('Name')
    expect(columns[1].key).toBe('age')
    expect(columns[1].header).toBe('Age')
  })

  it('should use custom render function', () => {
    const config = [
      { 
        key: 'name' as const, 
        header: 'Name',
        render: (item: { name: string }) => React.createElement('span', null, item.name)
      },
    ]
    
    const columns = useColumns(config, false)
    
    expect(columns[0].render?.({ name: 'John' }, false)).toBeDefined()
  })
})