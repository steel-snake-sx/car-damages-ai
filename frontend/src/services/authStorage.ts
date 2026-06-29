import type { AuthUser } from '../types/models'

const TOKEN_KEY = 'adc_admin_token'
const USER_KEY = 'adc_admin_user'

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setStoredToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token)
}

export function clearStoredToken(): void {
  localStorage.removeItem(TOKEN_KEY)
}

export function getStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) {
    return null
  }

  try {
    const parsed: unknown = JSON.parse(raw)
    if (!parsed || typeof parsed !== 'object') {
      return null
    }

    const email = (parsed as { email?: unknown }).email
    const role = (parsed as { role?: unknown }).role
    if (typeof email !== 'string' || (role !== 'Admin' && role !== 'Manager')) {
      return null
    }

    return { email, role }
  } catch {
    return null
  }
}

export function setStoredUser(user: AuthUser): void {
  localStorage.setItem(USER_KEY, JSON.stringify(user))
}

export function clearStoredUser(): void {
  localStorage.removeItem(USER_KEY)
}
