type Localizer = (key: string) => string

export function getSeverityLabel(severity: string, localize?: Localizer): string {
  const normalized = severity.trim().toLowerCase()

  if (localize) {
    const localized = localize(`severity.${normalized}`)
    if (localized !== `severity.${normalized}`) {
      return localized
    }
  }

  switch (normalized) {
    case 'low':
      return 'низкая'
    case 'medium':
      return 'средняя'
    case 'high':
      return 'высокая'
    default:
      return severity
  }
}
