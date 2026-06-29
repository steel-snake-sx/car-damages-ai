import { Button } from '../shared/Button'
import { useI18n } from '../../services/i18n'

type AISectionProps = {
  image: string
  onOpenRequest: () => void
}

export function AISection({ image, onOpenRequest }: AISectionProps) {
  const { t } = useI18n()

  return (
    <section className="ai-section" id="about">
      <div className="container">
        <div className="flex-ai">
          <div className="ai-img-wrap">
            <img src={image} alt="AI Logic" />
          </div>
          <div className="ai-content">
            <h2>{t('landing.aiTitle')}</h2>
            <p>{t('landing.aiParagraph1')}</p>
            <p>{t('landing.aiParagraph2')}</p>
            <Button variant="outline" onClick={onOpenRequest}>
              {t('landing.aiTryNow')}
            </Button>
          </div>
        </div>
      </div>
    </section>
  )
}
