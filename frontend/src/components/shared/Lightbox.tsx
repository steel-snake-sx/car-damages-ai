import { useI18n } from '../../services/i18n'

type LightboxProps = {
  open: boolean
  images: string[]
  index: number
  onClose: () => void
  onNavigate: (direction: -1 | 1) => void
}

export function Lightbox({ open, images, index, onClose, onNavigate }: LightboxProps) {
  const { t } = useI18n()

  if (!open || images.length === 0) {
    return null
  }

  const currentImage = images[index] ?? images[0]

  return (
    <div className="lightbox active" onClick={onClose}>
      <span className="close-modal" onClick={onClose}>
        ×
      </span>
      <div
        className="gal-nav left"
        style={{ position: 'fixed', left: 30 }}
        role="button"
        aria-label={t('common.back')}
        onClick={(event) => {
          event.stopPropagation()
          onNavigate(-1)
        }}
      >
        ❮
      </div>
      <img src={currentImage} onClick={(event) => event.stopPropagation()} />
      <div
        className="gal-nav right"
        style={{ position: 'fixed', right: 30 }}
        role="button"
        aria-label={t('form.next')}
        onClick={(event) => {
          event.stopPropagation()
          onNavigate(1)
        }}
      >
        ❯
      </div>
    </div>
  )
}
