import { useI18n } from '../../services/i18n'

type EmailStatusBadgeProps = {
  status: string
}

export function EmailStatusBadge({ status }: EmailStatusBadgeProps) {
  const { t } = useI18n()
  const normalized = status.trim().toLowerCase()

  const className =
    normalized === 'sent'
      ? 'st-ok'
      : normalized === 'failed'
        ? 'st-err'
        : normalized === 'pending'
          ? 'st-warn'
          : 'st-new'

  const label =
    normalized === 'sent'
      ? t('emails.statusSent')
      : normalized === 'failed'
        ? t('emails.statusFailed')
        : normalized === 'pending'
          ? t('emails.statusPending')
          : status

  return <span className={`status-pill ${className}`}>{label}</span>
}
