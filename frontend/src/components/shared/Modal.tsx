import type { ReactNode } from 'react'
import { MobileOverlay } from './MobileOverlay'

type ModalProps = {
  open: boolean
  onClose: () => void
  children: ReactNode
}

export function Modal({ open, onClose, children }: ModalProps) {
  if (!open) {
    return null
  }

  return (
    <>
      <MobileOverlay open={open} onClick={onClose} />
      <div className="modal" onClick={onClose}>
        <div className="modal-card" onClick={(event) => event.stopPropagation()}>
          <span className="close-modal" onClick={onClose}>
            ×
          </span>
          {children}
        </div>
      </div>
    </>
  )
}
