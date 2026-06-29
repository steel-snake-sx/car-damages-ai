import { useEffect, useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import { MobileOverlay } from '../components/shared/MobileOverlay'
import { useI18n } from '../services/i18n'
import { LanguageToggle } from '../components/shared/LanguageToggle'

type DashboardLayoutProps = {
  children: React.ReactNode
  onLogout: () => void
  canManageUsers: boolean
}

export function DashboardLayout({ children, onLogout, canManageUsers }: DashboardLayoutProps) {
  const [menuOpen, setMenuOpen] = useState(false)
  const { t } = useI18n()

  useEffect(() => {
    const previousOverflow = document.body.style.overflow

    if (menuOpen) {
      document.body.style.overflow = 'hidden'
    }

    return () => {
      document.body.style.overflow = previousOverflow
    }
  }, [menuOpen])

  return (
    <div className="admin-panel">
      <button className="mobile-menu-trigger" type="button" onClick={() => setMenuOpen(true)}>
        ☰ {t('nav.menu')}
      </button>

      <MobileOverlay open={menuOpen} onClick={() => setMenuOpen(false)} />

      {/* Боковое меню с навигацией по разделам */}
      <aside className={menuOpen ? 'sidebar is-open' : 'sidebar'}>
        <div className="logo">
          Auto<span>AI</span>
        </div>
        <NavLink
          to="/admin/dashboard"
          className={({ isActive }) => navClass(isActive)}
          onClick={() => setMenuOpen(false)}
        >
          📊 {t('nav.dashboard')}
        </NavLink>
        <NavLink
          to="/admin/requests"
          className={({ isActive }) => navClass(isActive)}
          onClick={() => setMenuOpen(false)}
        >
          📄 {t('nav.requests')}
        </NavLink>
        <NavLink
          to="/admin/emails"
          className={({ isActive }) => navClass(isActive)}
          onClick={() => setMenuOpen(false)}
        >
          ✉️ {t('nav.emails')}
        </NavLink>
        {canManageUsers ? (
          <NavLink
            to="/admin/users"
            className={({ isActive }) => navClass(isActive)}
            onClick={() => setMenuOpen(false)}
          >
            👥 {t('nav.users')}
          </NavLink>
        ) : null}
        <div className="sidebar-lang-wrap">
          <LanguageToggle className="lang-toggle-sidebar" />
        </div>
        <div style={{ flexGrow: 1 }} />
        <Link
          to="/"
          className="side-link"
          style={{ color: '#ff7b72', opacity: 0.8 }}
          onClick={() => {
            onLogout()
            setMenuOpen(false)
          }}
        >
          🚪 {t('nav.logout')} / {t('nav.website')}
        </Link>
      </aside>

      <main className="admin-main">{children}</main>
    </div>
  )
}

function navClass(isActive: boolean) {
  return isActive ? 'side-link active' : 'side-link'
}
