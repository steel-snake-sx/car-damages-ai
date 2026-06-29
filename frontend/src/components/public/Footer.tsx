import { useI18n } from '../../services/i18n'

type FooterProps = {
  onGoAdmin: () => void
}

export function Footer({ onGoAdmin }: FooterProps) {
  const { t } = useI18n()

  return (
    <footer>
      <div className="container">
        <div className="footer-flex">
          <div className="footer-copy">{t('landing.footerCopyright')}</div>
          <div className="footer-links">
            <span className="footer-link-item">{t('landing.footerPrivacy')}</span>
            <span className="admin-entry" onClick={onGoAdmin}>
              {t('landing.footerStaffLogin')}
            </span>
          </div>
        </div>
      </div>
    </footer>
  )
}
