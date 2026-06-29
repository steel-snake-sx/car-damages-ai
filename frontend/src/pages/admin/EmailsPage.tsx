import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table } from '../../components/shared/Table'
import { useI18n } from '../../services/i18n'
import type { EmailHistoryItem } from '../../types/models'
import { getEmailHistoryApi, resendEmailApi } from '../../services/requestsApi'
import { useToast } from '../../components/shared/ToastProvider'
import { EmailStatusBadge } from '../../components/shared/EmailStatusBadge'

export function EmailsPage() {
  const navigate = useNavigate()
  const { t, lang } = useI18n()
  const { showToast } = useToast()
  const locale = lang === 'en' ? 'en-US' : 'ru-RU'
  const [emailHistory, setEmailHistory] = useState<EmailHistoryItem[]>([])
  const [emailHistoryError, setEmailHistoryError] = useState('')
  const [emailSearch, setEmailSearch] = useState('')
  const [resendingId, setResendingId] = useState<string | null>(null)

  const loadEmailHistory = async () => {
    try {
      const response = await getEmailHistoryApi()
      setEmailHistory(response)
      setEmailHistoryError('')
    } catch (error) {
      setEmailHistory([])
      setEmailHistoryError(error instanceof Error ? error.message : t('emails.loadingError'))
    }
  }

  useEffect(() => {
    void loadEmailHistory()
  }, [t])

  const filteredEmailHistory = useMemo(() => {
    const query = emailSearch.trim().toLowerCase()
    if (!query) {
      return emailHistory
    }

    return emailHistory.filter((item) => {
      const statusLabel =
        item.status.toLowerCase() === 'sent'
          ? t('emails.statusSent').toLowerCase()
          : item.status.toLowerCase() === 'failed'
            ? t('emails.statusFailed').toLowerCase()
            : item.status.toLowerCase() === 'pending'
              ? t('emails.statusPending').toLowerCase()
              : item.status.toLowerCase()

      const fields = [item.recipientEmail, item.fullName, item.requestId, item.subject ?? '', item.status, statusLabel]

      return fields.some((field) => field.toLowerCase().includes(query))
    })
  }, [emailHistory, emailSearch, t])

  const resendEmail = async (id: string) => {
    setResendingId(id)
    try {
      await resendEmailApi(id)
      showToast(t('emails.resendSuccess'), 'success')
      await loadEmailHistory()
    } catch (error) {
      const message = error instanceof Error ? error.message : t('emails.resendError')
      showToast(message, 'error')
    } finally {
      setResendingId(null)
    }
  }

  return (
    <>
      <div className="top-inline-row">
        <h2 style={{ marginBottom: 0 }}>{t('emails.sentBlockTitle')}</h2>
        <input
          className="table-search"
          value={emailSearch}
          onChange={(event) => setEmailSearch(event.target.value)}
          placeholder={t('emails.searchPlaceholder')}
        />
      </div>

      {emailHistoryError ? <p className="form-error">{emailHistoryError}</p> : null}
      {!emailHistoryError && filteredEmailHistory.length === 0 ? <div className="page-loader">{t('emails.empty')}</div> : null}

      {!emailHistoryError && filteredEmailHistory.length > 0 ? (
        <Table
          columns={[
            {
              key: 'requestId',
              title: t('emails.requestId'),
              render: (item: EmailHistoryItem) => (
                <button
                  type="button"
                  className="table-link"
                  onClick={(event) => {
                    event.stopPropagation()
                    navigate(`/admin/requests/${item.requestId}`)
                  }}
                >
                  <span className="muted-id">{item.requestId}</span>
                </button>
              ),
            },
            { key: 'fullName', title: t('emails.fullName'), render: (item: EmailHistoryItem) => item.fullName },
            { key: 'recipientEmail', title: t('emails.recipient'), render: (item: EmailHistoryItem) => item.recipientEmail },
            { key: 'subject', title: t('emails.subject'), render: (item: EmailHistoryItem) => item.subject ?? '—' },
            { key: 'status', title: t('emails.status'), render: (item: EmailHistoryItem) => <EmailStatusBadge status={item.status} /> },
            {
              key: 'createdAt',
              title: t('emails.createdAt'),
              render: (item: EmailHistoryItem) => new Date(item.createdAt).toLocaleString(locale),
            },
            {
              key: 'actions',
              title: t('emails.actions'),
              render: (item: EmailHistoryItem) =>
                item.status.toLowerCase() === 'failed' ? (
                  <button
                    type="button"
                    className="btn btn-outline btn-sm"
                    disabled={resendingId === item.id}
                    onClick={(event) => {
                      event.stopPropagation()
                      void resendEmail(item.id)
                    }}
                  >
                    {resendingId === item.id ? t('emails.resending') : t('emails.resend')}
                  </button>
                ) : (
                  <span className="muted-id">—</span>
                ),
            },
          ]}
          rows={filteredEmailHistory}
          rowKey={(item) => item.id}
          onRowClick={(item) => navigate(`/admin/requests/${item.requestId}`)}
        />
      ) : null}
    </>
  )
}
