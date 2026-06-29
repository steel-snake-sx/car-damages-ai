import { Link } from 'react-router-dom'
import { assets } from '../../services/mockData'
import { useI18n } from '../../services/i18n'

export function NotFoundPage() {
  const { t } = useI18n()

  return (
    <main className="not-found-page">
      <div className="not-found-shell">
        <div className="not-found-visual" aria-hidden>
          <div className="not-found-code">404</div>
          <div className="not-found-track">
            <img src={assets.carLoading} alt="" className="not-found-car" />
            <span className="not-found-impact" />
          </div>
        </div>

        <div className="not-found-copy">
          <h1 className="not-found-title">{t('errors.notFoundTitle')}</h1>
          <p>{t('errors.notFound')}</p>
          <div className="not-found-actions">
            <Link to="/" className="btn btn-primary">
              {t('errors.returnHome')}
            </Link>
          </div>
        </div>
      </div>
    </main>
  )
}
