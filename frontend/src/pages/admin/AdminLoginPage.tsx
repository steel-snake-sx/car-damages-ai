import { useEffect, useState } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { Input } from '../../components/shared/Input'
import { Button } from '../../components/shared/Button'
import { useAppContext } from '../../services/appContext'
import { assets } from '../../services/mockData'
import { useToast } from '../../components/shared/ToastProvider'
import { useI18n } from '../../services/i18n'

export function AdminLoginPage() {
  const navigate = useNavigate()
  const { authUser, authLoading, login } = useAppContext()
  const { showToast } = useToast()
  const { t } = useI18n()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    setError('')
  }, [email, password])

  if (!authLoading && authUser) {
    return <Navigate to="/admin/dashboard" replace />
  }

  const submit = async () => {
    setSubmitting(true)
    setError('')

    try {
      const success = await login(email, password)
      if (success) {
        showToast(t('auth.success'), 'success')
        navigate('/admin/dashboard')
        return
      }

      setError(t('auth.errorInvalid'))
      showToast(t('auth.errorLoginToast'), 'error')
    } catch {
      setError(t('auth.errorLogin'))
      showToast(t('auth.errorLogin'), 'error')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div
      className="admin-login-screen"
      style={{
        backgroundImage: `linear-gradient(rgba(28, 28, 28, 0.92), rgba(28, 28, 28, 0.95)), url(${assets.login})`,
      }}
    >
      <div className="login-card">
        <h2>{t('auth.panelTitle')}</h2>
        <Input
          label={t('auth.email')}
          type="email"
          value={email}
          onChange={setEmail}
          placeholder="admin@autoai.com"
        />
        <Input
          label={t('auth.password')}
          type="password"
          value={password}
          onChange={setPassword}
          placeholder="••••••••"
        />
        <Button className="btn-block" onClick={() => void submit()} disabled={submitting}>
          {submitting ? t('auth.loggingIn') : t('auth.login')}
        </Button>
        {error ? <p className="form-error">{error}</p> : null}
        <div className="login-back">
          <Link to="/">← {t('auth.backToSite')}</Link>
        </div>
      </div>
    </div>
  )
}
