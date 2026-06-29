import { useEffect, useMemo, useState } from 'react'
import { useI18n } from '../../services/i18n'

type GalleryProps = {
  images: string[]
  onOpenLightbox: (index: number) => void
}

export function Gallery({ images, onOpenLightbox }: GalleryProps) {
  const { t } = useI18n()
  const [index, setIndex] = useState(0)

  useEffect(() => {
    setIndex(0)
  }, [images])

  const currentImage = useMemo(() => images[index] ?? images[0] ?? '', [images, index])

  if (images.length === 0) {
    return <div className="empty-gallery">{t('requests.noPhotos')}</div>
  }

  const move = (direction: -1 | 1) => {
    setIndex((prev) => (prev + direction + images.length) % images.length)
  }

  return (
    <>
      <div className="gallery-main-wrapper">
        <div
          className="gallery-main"
          style={{ backgroundImage: `url(${currentImage})` }}
          onClick={() => onOpenLightbox(index)}
        />
        <div className="gal-nav left" onClick={() => move(-1)} role="button" aria-label={t('common.back')}>
          ❮
        </div>
        <div className="gal-nav right" onClick={() => move(1)} role="button" aria-label={t('form.next')}>
          ❯
        </div>
      </div>
      <div className="gallery-thumbs">
        {images.map((image, imageIndex) => (
          <button
            key={`${image}-${imageIndex}`}
            className={imageIndex === index ? 'thumb active' : 'thumb'}
            style={{ backgroundImage: `url(${image})` }}
            onClick={() => setIndex(imageIndex)}
            type="button"
          />
        ))}
      </div>
    </>
  )
}
