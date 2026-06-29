import type { ReactNode } from 'react'

type ButtonVariant = 'primary' | 'outline' | 'success' | 'danger'

type ButtonProps = {
  children: ReactNode
  type?: 'button' | 'submit'
  variant?: ButtonVariant
  className?: string
  onClick?: () => void
  disabled?: boolean
}

export function Button({
  children,
  type = 'button',
  variant = 'primary',
  className,
  onClick,
  disabled,
}: ButtonProps) {
  const variantClass =
    variant === 'primary'
      ? 'btn btn-primary'
      : variant === 'outline'
        ? 'btn btn-outline'
        : variant === 'success'
          ? 'btn btn-success'
          : 'btn btn-danger'

  const classes = className ? `${variantClass} ${className}` : variantClass

  return (
    <button type={type} className={classes} onClick={onClick} disabled={disabled}>
      {children}
    </button>
  )
}
