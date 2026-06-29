import type { RequestStatus } from '../../types/models'
import { getStatusClass, getStatusLabel } from '../../services/statusUtils'
import { useI18n } from '../../services/i18n'

type StatusBadgeProps = {
  status: RequestStatus
}

export function StatusBadge({ status }: StatusBadgeProps) {
  const { t } = useI18n()

  return <span className={`status-pill ${getStatusClass(status)}`}>{getStatusLabel(status, t)}</span>
}
