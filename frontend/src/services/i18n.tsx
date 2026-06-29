import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { en } from '../locales/en'
import { ru } from '../locales/ru'
import type { Language, LocaleDictionary } from '../locales/types'

const STORAGE_KEY = 'lang'

const dictionaries: Record<Language, LocaleDictionary> = {
  ru,
  en,
}

type I18nContextValue = {
  lang: Language
  setLang: (lang: Language) => void
  t: (key: string, params?: Record<string, string | number>) => string
}

const I18nContext = createContext<I18nContextValue | undefined>(undefined)

type I18nProviderProps = {
  children: React.ReactNode
}

export function I18nProvider({ children }: I18nProviderProps) {
  const [lang, setLangState] = useState<Language>(() => getStoredLanguage())

  useEffect(() => {
    window.localStorage.setItem(STORAGE_KEY, lang)
  }, [lang])

  const setLang = useCallback((next: Language) => {
    setLangState(next)
  }, [])

  const t = useCallback(
    (key: string, params?: Record<string, string | number>) => {
      const template = resolveTranslation(dictionaries[lang], key) ?? resolveTranslation(dictionaries.ru, key) ?? key
      if (!params) {
        return template
      }

      return Object.entries(params).reduce((acc, [paramKey, value]) => {
        return acc.split(`{${paramKey}}`).join(String(value))
      }, template)
    },
    [lang],
  )

  const value = useMemo<I18nContextValue>(
    () => ({
      lang,
      setLang,
      t,
    }),
    [lang, setLang, t],
  )

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}

export function useI18n() {
  const context = useContext(I18nContext)
  if (!context) {
    throw new Error('useI18n must be used inside I18nProvider')
  }

  return context
}

export function getCurrentLanguage(): Language {
  if (typeof window === 'undefined') {
    return 'ru'
  }

  return getStoredLanguage()
}

function getStoredLanguage(): Language {
  if (typeof window === 'undefined') {
    return 'ru'
  }

  const stored = window.localStorage.getItem(STORAGE_KEY)
  if (stored === 'en' || stored === 'ru') {
    return stored
  }

  return 'ru'
}

function resolveTranslation(dictionary: LocaleDictionary, key: string): string | null {
  const chunks = key.split('.')
  let current: string | LocaleDictionary | undefined = dictionary

  for (const chunk of chunks) {
    if (!current || typeof current === 'string') {
      return null
    }

    current = current[chunk] as string | LocaleDictionary | undefined
  }

  return typeof current === 'string' ? current : null
}
