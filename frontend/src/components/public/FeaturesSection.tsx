import { useI18n } from '../../services/i18n'

export function FeaturesSection() {
  const { t } = useI18n()

  return (
    <section className="features">
      <div className="container">
        <div className="grid-3">
          <div className="feature-card">
            <span className="icon">⚡</span>
            <h3>{t('landing.featuresFastTitle')}</h3>
            <p>{t('landing.featuresFastText')}</p>
          </div>
          <div className="feature-card">
            <span className="icon">🎯</span>
            <h3>{t('landing.featuresAccurateTitle')}</h3>
            <p>{t('landing.featuresAccurateText')}</p>
          </div>
          <div className="feature-card">
            <span className="icon">📱</span>
            <h3>{t('landing.featuresEasyTitle')}</h3>
            <p>{t('landing.featuresEasyText')}</p>
          </div>
        </div>
      </div>
    </section>
  )
}
