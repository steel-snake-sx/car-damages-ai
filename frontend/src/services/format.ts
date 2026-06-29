export function maskRuPhone(input: string): string {
  const digits = input.replace(/\D/g, '')

  let normalized = digits
  if (normalized.startsWith('8')) {
    normalized = `7${normalized.slice(1)}`
  }
  if (!normalized.startsWith('7') && normalized.length > 0) {
    normalized = `7${normalized}`
  }

  const part = normalized.slice(1, 11)
  const p1 = part.slice(0, 3)
  const p2 = part.slice(3, 6)
  const p3 = part.slice(6, 8)
  const p4 = part.slice(8, 10)

  let result = '+7'
  if (p1) {
    result += ` (${p1}`
  }
  if (p1.length === 3) {
    result += ')'
  }
  if (p2) {
    result += ` ${p2}`
  }
  if (p3) {
    result += `-${p3}`
  }
  if (p4) {
    result += `-${p4}`
  }

  return result
}

export function isValidRuPhone(value: string): boolean {
  return /^\+7 \(\d{3}\) \d{3}-\d{2}-\d{2}$/.test(value)
}
