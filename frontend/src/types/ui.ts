import type { ReactNode } from 'react'

export interface TableColumn<T> {
  key: string
  title: string
  render: (row: T) => ReactNode
}
