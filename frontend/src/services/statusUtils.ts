import type { RequestStatus } from '../types/models'

type Localizer = (key: string) => string

export function getStatusLabel(status: RequestStatus, localize?: Localizer): string {
  if (localize) {
    const localized = localize(`status.${status}`)
    if (localized !== `status.${status}`) {
      return localized
    }
  }

  switch (status) {
    case 'New':
      return 'Новый'
    case 'AiProcessed':
      return 'Обработано AI'
    case 'Approved':
      return 'Одобрено'
    case 'Rejected':
      return 'Отклонено'
    case 'Notified':
      return 'Уведомлен'
    default:
      return status
  }
}

export function getStatusClass(status: RequestStatus): string {
  switch (status) {
    case 'New':
      return 'st-new'
    case 'AiProcessed':
      return 'st-ai'
    case 'Approved':
      return 'st-ok'
    case 'Notified':
      return 'st-ok'
    case 'Rejected':
      return 'st-err'
    default:
      return 'st-new'
  }
}

export function getStatusEditOptions(localize?: Localizer): Array<{ value: RequestStatus; label: string }> {
  return [
    { value: 'New', label: getStatusLabel('New', localize) },
    { value: 'AiProcessed', label: getStatusLabel('AiProcessed', localize) },
    { value: 'Approved', label: getStatusLabel('Approved', localize) },
    { value: 'Rejected', label: getStatusLabel('Rejected', localize) },
    { value: 'Notified', label: getStatusLabel('Notified', localize) },
  ]
}
