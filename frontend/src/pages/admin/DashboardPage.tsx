import { Link, useNavigate } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { Table } from '../../components/shared/Table'
import { StatusBadge } from '../../components/shared/StatusBadge'
import { useAppContext } from '../../services/appContext'
import { getEmailHistoryApi } from '../../services/requestsApi'
import type { DamageRequest, EmailHistoryItem } from '../../types/models'
import { useI18n } from '../../services/i18n'
import { EmailStatusBadge } from '../../components/shared/EmailStatusBadge'

export function DashboardPage() {
  const navigate = useNavigate()
  const { requests, requestsLoading, requestsError } = useAppContext()
  const { t, lang } = useI18n()
  const locale = lang === 'en' ? 'en-US' : 'ru-RU'
  const [emailHistory, setEmailHistory] = useState<EmailHistoryItem[]>([])
  const [emailHistoryError, setEmailHistoryError] = useState('')

  const requestItems: DamageRequest[] = Array.isArray(requests)
    ? requests
    : requests && typeof requests === 'object' && Array.isArray((requests as { items?: DamageRequest[] }).items)
      ? (requests as { items: DamageRequest[] }).items
      : []

  const stats = {
    ai: requestItems.filter((request) => request.status === 'AiProcessed').length,
    ok: requestItems.filter((request) => request.status === 'Approved' || request.status === 'Notified').length,
    err: requestItems.filter((request) => request.status === 'Rejected').length,
  }

  const previewRows = [...requestItems]
    .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
    .slice(0, 4)

  useEffect(() => {
    const loadEmailHistory = async () => {
      try {
        const response = await getEmailHistoryApi()
        setEmailHistory(
          [...response]
            .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
            .slice(0, 4),
        )
        setEmailHistoryError('')
      } catch (error) {
        setEmailHistory([])
        setEmailHistoryError(error instanceof Error ? error.message : t('emails.loadingError'))
      }
    }

    void loadEmailHistory()
  }, [t])

  return (
    <>
      <div className="top-inline-row">
        <h2>{t('dashboard.summary')}</h2>
      </div>
      <div
        className="stat-grid"
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(3, minmax(0, 1fr))',
          gap: 18,
        }}
      >
        <div
          className="stat-card"
          style={{
            padding: '18px',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            textAlign: 'center',
            transition: 'transform 0.2s ease, box-shadow 0.2s ease',
          }}
          onMouseEnter={(event) => {
            event.currentTarget.style.transform = 'translateY(-2px)'
            event.currentTarget.style.boxShadow = '0 8px 18px rgba(0, 0, 0, 0.12)'
          }}
          onMouseLeave={(event) => {
            event.currentTarget.style.transform = 'translateY(0)'
            event.currentTarget.style.boxShadow = ''
          }}
        >
          <span className="label" style={{ fontSize: 14, opacity: 0.7 }}>
            {t('dashboard.aiProcessed')}
          </span>
          <div className="value" style={{ fontSize: 34, fontWeight: 700, marginTop: 8 }}>
            {stats.ai}
          </div>
        </div>
        <div
          className="stat-card"
          style={{
            padding: '18px',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            textAlign: 'center',
            transition: 'transform 0.2s ease, box-shadow 0.2s ease',
          }}
          onMouseEnter={(event) => {
            event.currentTarget.style.transform = 'translateY(-2px)'
            event.currentTarget.style.boxShadow = '0 8px 18px rgba(0, 0, 0, 0.12)'
          }}
          onMouseLeave={(event) => {
            event.currentTarget.style.transform = 'translateY(0)'
            event.currentTarget.style.boxShadow = ''
          }}
        >
          <span className="label" style={{ fontSize: 14, opacity: 0.7 }}>
            {t('dashboard.approved')}
          </span>
          <div className="value" style={{ fontSize: 34, fontWeight: 700, marginTop: 8 }}>
            {stats.ok}
          </div>
        </div>
        <div
          className="stat-card"
          style={{
            padding: '18px',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            textAlign: 'center',
            transition: 'transform 0.2s ease, box-shadow 0.2s ease',
          }}
          onMouseEnter={(event) => {
            event.currentTarget.style.transform = 'translateY(-2px)'
            event.currentTarget.style.boxShadow = '0 8px 18px rgba(0, 0, 0, 0.12)'
          }}
          onMouseLeave={(event) => {
            event.currentTarget.style.transform = 'translateY(0)'
            event.currentTarget.style.boxShadow = ''
          }}
        >
          <span className="label" style={{ fontSize: 14, opacity: 0.7 }}>
            {t('dashboard.rejected')}
          </span>
          <div className="value" style={{ fontSize: 34, fontWeight: 700, marginTop: 8 }}>
            {stats.err}
          </div>
        </div>
      </div>

      <div className="top-inline-row dashboard-secondary-row">
        <h3 className="sub-title">{t('dashboard.latestPreview')}</h3>
        <Link to="/admin/requests" className="inline-view-all">
          {t('nav.viewAllRequests')} →
        </Link>
      </div>

      {requestsLoading ? <div className="page-loader">{t('dashboard.loadingRequests')}</div> : null}
      {!requestsLoading && requestsError ? <p className="form-error">{requestsError}</p> : null}
      {!requestsLoading && !requestsError && previewRows.length === 0 ? (
        <div className="page-loader">{t('dashboard.noRequests')}</div>
      ) : null}

      {!requestsLoading && !requestsError && previewRows.length > 0 ? (
        <Table
          columns={[
            {
              key: 'customer',
              title: t('dashboard.customer'),
              render: (request: DamageRequest) => request.fullName,
            },
            {
              key: 'car',
              title: t('dashboard.vehicle'),
              render: (request: DamageRequest) => `${request.carBrand} ${request.carModel}`,
            },
            {
              key: 'status',
              title: t('dashboard.status'),
              render: (request: DamageRequest) => <StatusBadge status={request.status} />,
            },
            {
              key: 'createdAt',
              title: t('dashboard.date'),
              render: (request: DamageRequest) => new Date(request.createdAt).toLocaleString(locale),
            },
          ]}
          rows={previewRows}
          rowKey={(request) => request.id}
          onRowClick={(request) => navigate(`/admin/requests/${request.id}`)}
        />
      ) : null}

      <div className="top-inline-row dashboard-secondary-row">
        <h3 className="sub-title">{t('emails.sentBlockTitle')}</h3>
        <Link to="/admin/emails" className="inline-view-all">
          {t('nav.viewAllEmails')} →
        </Link>
      </div>

      {emailHistoryError ? <p className="form-error">{emailHistoryError}</p> : null}
      {!emailHistoryError && emailHistory.length === 0 ? <div className="page-loader">{t('emails.empty')}</div> : null}

      {!emailHistoryError && emailHistory.length > 0 ? (
        <Table
          columns={[
            { key: 'fullName', title: t('emails.fullName'), render: (item: EmailHistoryItem) => item.fullName },
            { key: 'recipientEmail', title: t('emails.recipient'), render: (item: EmailHistoryItem) => item.recipientEmail },
            { key: 'subject', title: t('emails.subject'), render: (item: EmailHistoryItem) => item.subject ?? '—' },
            { key: 'status', title: t('emails.status'), render: (item: EmailHistoryItem) => <EmailStatusBadge status={item.status} /> },
            { key: 'createdAt', title: t('emails.createdAt'), render: (item: EmailHistoryItem) => new Date(item.createdAt).toLocaleString(locale) },
          ]}
          rows={emailHistory}
          rowKey={(item) => item.id}
          onRowClick={(item) => navigate(`/admin/requests/${item.requestId}`)}
        />
      ) : null}
    </>
  )
}
