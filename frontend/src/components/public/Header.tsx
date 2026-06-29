import { useEffect, useRef, useState } from 'react'
import { Button } from '../shared/Button'
import { LanguageToggle } from '../shared/LanguageToggle'
import { MobileOverlay } from '../shared/MobileOverlay'
import { useI18n } from '../../services/i18n'

type HeaderProps = {
  onOpenRequest: () => void
}

export function Header({ onOpenRequest }: HeaderProps) {
  const { t } = useI18n()
  const [menuOpen, setMenuOpen] = useState(false)
  const touchStartX = useRef(0)
  const touchStartY = useRef(0)

  useEffect(() => {
    const previousOverflow = document.body.style.overflow

    if (menuOpen) {
      document.body.style.overflow = 'hidden'
    }

    return () => {
      document.body.style.overflow = previousOverflow
    }
  }, [menuOpen])

  const closeMenu = () => {
    setMenuOpen(false)
  }

  const handleOpenRequest = () => {
    closeMenu()
    onOpenRequest()
  }

  const onDrawerTouchStart = (event: React.TouchEvent<HTMLElement>) => {
    const touch = event.touches[0]
    if (!touch) {
      return
    }

    touchStartX.current = touch.clientX
    touchStartY.current = touch.clientY
  }

  const onDrawerTouchEnd = (event: React.TouchEvent<HTMLElement>) => {
    const touch = event.changedTouches[0]
    if (!touch) {
      return
    }

    const deltaX = touch.clientX - touchStartX.current
    const deltaY = Math.abs(touch.clientY - touchStartY.current)
    if (deltaX > 64 && deltaY < 64) {
      closeMenu()
    }
  }

  const navContent = (
    <>
      <a href="#works" onClick={closeMenu}>
        {t('nav.howItWorks')}
      </a>
      <a href="#about" onClick={closeMenu}>
        {t('nav.about')}
      </a>
      <LanguageToggle />
      <Button onClick={handleOpenRequest}>{t('nav.leaveRequest')}</Button>
    </>
  )

  return (
    <header className={menuOpen ? 'site-header is-menu-open' : 'site-header'}>
      <div className="container site-header-inner">
        <div className="logo">
          Auto<span>AI</span>
        </div>

        <nav className="site-nav site-nav-desktop">{navContent}</nav>

        <button className="site-menu-trigger" type="button" onClick={() => setMenuOpen(true)}>
          ☰
        </button>
      </div>

      <MobileOverlay open={menuOpen} onClick={closeMenu} />

      <nav
        className={menuOpen ? 'site-nav-drawer open' : 'site-nav-drawer'}
        onTouchStart={onDrawerTouchStart}
        onTouchEnd={onDrawerTouchEnd}
      >
        <button className="site-nav-close" type="button" onClick={closeMenu} aria-label={t('common.close')}>
          ×
        </button>
        {navContent}
      </nav>
    </header>
  )
}
