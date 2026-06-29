import { Navigate, Route, Routes } from 'react-router-dom'
import { LandingPage } from '../pages/public/LandingPage'
import { NotFoundPage } from '../pages/public/NotFoundPage'
import { AdminLoginPage } from '../pages/admin/AdminLoginPage'
import { DashboardPage } from '../pages/admin/DashboardPage'
import { RequestsPage } from '../pages/admin/RequestsPage'
import { RequestDetailsPage } from '../pages/admin/RequestDetailsPage'
import { UsersPage } from '../pages/admin/UsersPage'
import { AdminRootLayout } from '../pages/admin/AdminRootLayout'
import { EmailsPage } from '../pages/admin/EmailsPage'
import { useAppContext } from '../services/appContext'
import { useI18n } from '../services/i18n'

function UsersRouteGuard() {
  const { authUser, authLoading } = useAppContext()
  const { t } = useI18n()

  if (authLoading) {
    return <div className="page-loader">{t('common.loading')}</div>
  }

  if (authUser?.role !== 'Admin') {
    return <Navigate to="/admin/dashboard" replace />
  }

  return <UsersPage />
}

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<LandingPage />} />

      <Route path="/admin/login" element={<AdminLoginPage />} />

      <Route path="/admin" element={<AdminRootLayout />}>
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="requests" element={<RequestsPage />} />
        <Route path="emails" element={<EmailsPage />} />
        <Route path="requests/:id" element={<RequestDetailsPage />} />
        <Route path="users" element={<UsersRouteGuard />} />
        <Route index element={<Navigate to="dashboard" replace />} />
      </Route>
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}
