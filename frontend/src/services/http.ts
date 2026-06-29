import { getStoredToken } from './authStorage'
import { getCurrentLanguage } from './i18n'
import { en } from '../locales/en'
import { ru } from '../locales/ru'
import type { LocaleDictionary } from '../locales/types'

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

type RequestOptions = {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  body?: unknown
  auth?: boolean
  formData?: boolean
}

type JsonObject = Record<string, unknown>

export async function httpRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, auth = true, formData = false } = options

  const headers = new Headers()

  if (auth) {
    const token = getStoredToken()
    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }
  }

  headers.set('X-Language', getCurrentLanguage())

  let requestBody: BodyInit | undefined
  if (body !== undefined) {
    if (formData) {
      if (!(body instanceof FormData)) {
        throw new Error('Form data body must be FormData instance')
      }

      requestBody = body
    } else {
      headers.set('Content-Type', 'application/json')
      requestBody = JSON.stringify(body)
    }
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers,
    body: requestBody,
  })

  if (!response.ok) {
    const message = await extractErrorMessage(response)
    throw new ApiError(message, response.status)
  }

  if (response.status === 204 || response.headers.get('content-length') === '0') {
    return null as T
  }

  return (await response.json()) as T
}

export async function httpDownload(path: string): Promise<{ blob: Blob; filename: string }> {
  const headers = new Headers()
  const token = getStoredToken()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  headers.set('X-Language', getCurrentLanguage())

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'GET',
    headers,
  })

  if (!response.ok) {
    const message = await extractErrorMessage(response)
    throw new ApiError(message, response.status)
  }

  const disposition = response.headers.get('Content-Disposition')
    ?? response.headers.get('content-disposition')
  const filename = parseContentDispositionFilename(disposition) ?? 'download'

  return {
    blob: await response.blob(),
    filename,
  }
}

function decodeFileName(raw: string): string {
  const normalized = raw.trim().replace(/^UTF-8''/i, '').replace(/^"|"$/g, '')

  try {
    return decodeURIComponent(normalized)
  } catch {
    return normalized
  }
}

function parseContentDispositionFilename(disposition: string | null): string | null {
  if (!disposition) {
    return null
  }

  const utf8Match = disposition.match(/filename\*=UTF-8''([^;]+)/i)
  if (utf8Match?.[1]) {
    return decodeFileName(utf8Match[1])
  }

  const regularMatch = disposition.match(/filename=("[^"]+"|[^;]+)/i)
  if (regularMatch?.[1]) {
    return decodeFileName(regularMatch[1])
  }

  return null
}

async function extractErrorMessage(response: Response): Promise<string> {
  const contentType = response.headers.get('content-type') ?? ''
  const fallback = fallbackErrorText(response.status)

  if (contentType.includes('application/json')) {
    const payload: unknown = await response.json()

    if (!payload || typeof payload !== 'object') {
      return fallback
    }

    const payloadObject = payload as JsonObject

    const errors = payloadObject.errors
    if (errors && typeof errors === 'object') {
      for (const value of Object.values(errors)) {
        if (Array.isArray(value)) {
          const first = value.find((item) => typeof item === 'string' && item.trim())
          if (typeof first === 'string') {
            return first === 'Invalid value.' ? fallbackErrorText(400) : first
          }
        }
      }
    }

    if (typeof payloadObject.message === 'string' && payloadObject.message.trim()) {
      return payloadObject.message
    }

    if (typeof payloadObject.title === 'string' && payloadObject.title.trim()) {
      return payloadObject.title
    }

    return fallback
  }

  const text = await response.text()
  return text || fallback
}

function fallbackErrorText(status: number): string {
  const lang = getCurrentLanguage()
  const dictionary: LocaleDictionary = lang === 'en' ? en : ru

  if (status === 401) {
    return resolveLocaleKey(dictionary, 'errors.unauthorized')
  }

  if (status === 400) {
    return resolveLocaleKey(dictionary, 'errors.validationFailed')
  }

  return resolveLocaleKey(dictionary, 'errors.unknown')
}

function resolveLocaleKey(dictionary: LocaleDictionary, key: string): string {
  const parts = key.split('.')
  let current: unknown = dictionary

  for (const part of parts) {
    if (!current || typeof current !== 'object') {
      return key
    }

    current = (current as Record<string, unknown>)[part]
  }

  return typeof current === 'string' ? current : key
}

export function getApiBaseUrl(): string {
  return API_BASE_URL
}
