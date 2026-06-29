import { Navigate, Outlet } from 'react-router-dom'
import { DashboardLayout } from '../../layouts/DashboardLayout'
import { useAppContext } from '../../services/appContext'
import { useI18n } from '../../services/i18n'

export function AdminRootLayout() {
  const { authUser, authLoading, logout } = useAppContext()
  const { t } = useI18n()

  if (authLoading) {
    return <div className="page-loader">{t('common.loading')}</div>
  }

  if (!authUser) {
    return <Navigate to="/admin/login" replace />
  }

  return (
    <DashboardLayout
      canManageUsers={authUser.role === 'Admin'}
      onLogout={() => {
        logout()
      }}
    >
      <Outlet />
    </DashboardLayout>
  )
}
