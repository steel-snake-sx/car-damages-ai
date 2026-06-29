import { Button } from '../shared/Button'
import { useI18n } from '../../services/i18n'

type HeroSectionProps = {
  backgroundUrl: string
  onOpenRequest: () => void
}

export function HeroSection({ backgroundUrl, onOpenRequest }: HeroSectionProps) {
  const { t } = useI18n()

  return (
    <section
      className="hero"
      style={{
        backgroundImage: `linear-gradient(rgba(20, 20, 20, 0.85), rgba(20, 20, 20, 0.7)), url(${backgroundUrl})`,
      }}
    >
      <div className="hero-content">
        <h1>{t('landing.heroTitle')}</h1>
        <p>{t('landing.heroSubtitle')}</p>
        <Button className="hero-main-btn" onClick={onOpenRequest}>
          {t('nav.leaveRequest')}
        </Button>
      </div>
    </section>
  )
}
