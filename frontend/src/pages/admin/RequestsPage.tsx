import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Table } from '../../components/shared/Table'
import { StatusBadge } from '../../components/shared/StatusBadge'
import { useAppContext } from '../../services/appContext'
import type { DamageRequest } from '../../types/models'
import { useI18n } from '../../services/i18n'

export function RequestsPage() {
  const navigate = useNavigate()
  const {
    requests,
    requestsLoading,
    requestsError,
    currentRequestQuery,
    setRequestsQuery,
    exportRequestsExcel,
    authUser,
  } = useAppContext()
  const { t, lang } = useI18n()
  const locale = lang === 'en' ? 'en-US' : 'ru-RU'
  const requestItems: DamageRequest[] = requests
  const [searchInput, setSearchInput] = useState(currentRequestQuery.search ?? '')
  const [exporting, setExporting] = useState(false)
  const canExportExcel = authUser?.role === 'Admin' || authUser?.role === 'Manager'

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setRequestsQuery({ search: searchInput })
    }, 350)

    return () => {
      window.clearTimeout(timer)
    }
  }, [searchInput, setRequestsQuery])

  const sortedLabel = useMemo(() => {
    const direction = currentRequestQuery.sortDirection === 'asc' ? '↑' : '↓'
    return direction
  }, [currentRequestQuery.sortDirection])

  const onSort = (sortBy: 'createdAt' | 'status' | 'customer' | 'car') => {
    const nextDirection =
      currentRequestQuery.sortBy === sortBy && currentRequestQuery.sortDirection === 'desc' ? 'asc' : 'desc'

    setRequestsQuery({ sortBy, sortDirection: nextDirection })
  }

  const exportExcel = async () => {
    setExporting(true)
    try {
      await exportRequestsExcel()
    } finally {
      setExporting(false)
    }
  }

  return (
    <>
      <div className="top-inline-row">
        <h2 style={{ marginBottom: 0 }}>{t('requests.title')}</h2>
        <div className="top-inline-row compact-actions">
          <input
            className="table-search"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
            placeholder={t('requests.searchPlaceholder')}
          />
          {canExportExcel ? (
            <button className="btn btn-outline" type="button" onClick={() => void exportExcel()} disabled={exporting}>
              {exporting ? t('requests.exporting') : t('requests.exportExcel')}
            </button>
          ) : null}
        </div>
      </div>

      <div className="sort-row">
        <button className="sort-chip" type="button" onClick={() => onSort('createdAt')}>
          {t('requests.sortDate')} {currentRequestQuery.sortBy === 'createdAt' ? sortedLabel : ''}
        </button>
        <button className="sort-chip" type="button" onClick={() => onSort('status')}>
          {t('requests.sortStatus')} {currentRequestQuery.sortBy === 'status' ? sortedLabel : ''}
        </button>
        <button className="sort-chip" type="button" onClick={() => onSort('customer')}>
          {t('requests.sortCustomer')} {currentRequestQuery.sortBy === 'customer' ? sortedLabel : ''}
        </button>
        <button className="sort-chip" type="button" onClick={() => onSort('car')}>
          {t('requests.sortCar')} {currentRequestQuery.sortBy === 'car' ? sortedLabel : ''}
        </button>
      </div>

      {requestsLoading ? <div className="page-loader">{t('requests.loading')}</div> : null}
      {!requestsLoading && requestsError ? <p className="form-error">{requestsError}</p> : null}

      {!requestsLoading && !requestsError && requestItems.length === 0 ? (
        <div className="page-loader">{t('requests.notFound')}</div>
      ) : null}

      {!requestsLoading && !requestsError && requestItems.length > 0 ? (
        <Table
          columns={[
            {
              key: 'id',
              title: t('requests.id'),
              render: (request: DamageRequest) => <span className="muted-id">{request.id}</span>,
            },
            {
              key: 'customer',
              title: t('requests.customer'),
              render: (request: DamageRequest) => request.fullName,
            },
            {
              key: 'car',
              title: t('requests.car'),
              render: (request: DamageRequest) => `${request.carBrand} ${request.carModel}`,
            },
            {
              key: 'status',
              title: t('requests.status'),
              render: (request: DamageRequest) => <StatusBadge status={request.status} />,
            },
            {
              key: 'createdAt',
              title: t('requests.date'),
              render: (request: DamageRequest) => new Date(request.createdAt).toLocaleString(locale),
            },
          ]}
          rows={requestItems}
          rowKey={(request) => request.id}
          onRowClick={(request) => navigate(`/admin/requests/${request.id}`)}
        />
      ) : null}
    </>
  )
}
