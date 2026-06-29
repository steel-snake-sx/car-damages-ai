import { useI18n } from '../../services/i18n'

type LanguageToggleProps = {
  className?: string
}

export function LanguageToggle({ className }: LanguageToggleProps) {
  const { lang, setLang, t } = useI18n()

  const classes = className ? `lang-toggle ${className}` : 'lang-toggle'

  return (
    <div className={classes}>
      <button
        type="button"
        className={lang === 'ru' ? 'lang-btn active' : 'lang-btn'}
        onClick={() => setLang('ru')}
      >
        {t('lang.ru')}
      </button>
      <button
        type="button"
        className={lang === 'en' ? 'lang-btn active' : 'lang-btn'}
        onClick={() => setLang('en')}
      >
        {t('lang.en')}
      </button>
    </div>
  )
}
