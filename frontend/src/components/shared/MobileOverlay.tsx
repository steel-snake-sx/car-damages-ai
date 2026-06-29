type MobileOverlayProps = {
  open: boolean
  onClick: () => void
}

export function MobileOverlay({ open, onClick }: MobileOverlayProps) {
  if (!open) {
    return null
  }

  return <div className="mobile-overlay" onClick={onClick} />
}
