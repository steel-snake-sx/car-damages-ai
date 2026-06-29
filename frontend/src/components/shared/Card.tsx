import type { ReactNode } from 'react'

type CardProps = {
  children: ReactNode
  className?: string
}

export function Card({ children, className }: CardProps) {
  return <div className={className ? `card-details ${className}` : 'card-details'}>{children}</div>
}
