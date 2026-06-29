import { useMemo, useState } from 'react'
import { Modal } from '../../components/shared/Modal'
import { Button } from '../../components/shared/Button'
import { Input } from '../../components/shared/Input'
import { Table } from '../../components/shared/Table'
import { useAppContext } from '../../services/appContext'
import type { AdminUser, CreateAdminUserPayload, UpdateAdminUserPayload, UserRole } from '../../types/models'
import { useToast } from '../../components/shared/ToastProvider'
import { useI18n } from '../../services/i18n'

const initialCreateForm: CreateAdminUserPayload = {
  firstName: '',
  lastName: '',
  middleName: '',
  role: 'Manager',
  email: '',
  password: '',
  isActive: true,
}

const initialEditForm: UpdateAdminUserPayload = {
  firstName: '',
  lastName: '',
  middleName: '',
  role: 'Manager',
  email: '',
  isActive: true,
  password: '',
}

export function UsersPage() {
  const { users, usersLoading, usersError, createUser, updateUser } = useAppContext()
  const { showToast } = useToast()
  const { t, lang } = useI18n()
  const locale = lang === 'en' ? 'en-US' : 'ru-RU'

  const [createOpen, setCreateOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [createForm, setCreateForm] = useState<CreateAdminUserPayload>(initialCreateForm)
  const [editForm, setEditForm] = useState<UpdateAdminUserPayload>(initialEditForm)
  const [editingUserId, setEditingUserId] = useState<string | null>(null)

  const editingUser = useMemo(
    () => users.find((item) => item.id === editingUserId) ?? null,
    [editingUserId, users],
  )

  const openEdit = (user: AdminUser) => {
    setEditingUserId(user.id)
    setEditForm({
      firstName: user.firstName,
      lastName: user.lastName,
      middleName: user.middleName ?? '',
      role: user.role,
      email: user.email,
      isActive: user.isActive,
      password: '',
    })
    setError('')
    setEditOpen(true)
  }

  const create = async () => {
    if (!createForm.firstName || !createForm.lastName || !createForm.email || !createForm.password) {
      const message = t('users.requiredCreate')
      setError(message)
      showToast(message, 'error')
      return
    }

    setSubmitting(true)
    setError('')

    try {
      await createUser(createForm)
      setCreateOpen(false)
      setCreateForm(initialCreateForm)
      showToast(t('users.createSuccess'), 'success')
    } catch {
      const message = t('users.createError')
      setError(message)
      showToast(message, 'error')
    } finally {
      setSubmitting(false)
    }
  }

  const saveEdit = async () => {
    if (!editingUser || !editingUserId) {
      return
    }

    if (!editForm.firstName || !editForm.lastName || !editForm.email) {
      const message = t('users.requiredEdit')
      setError(message)
      showToast(message, 'error')
      return
    }

    setSubmitting(true)
    setError('')

    try {
      const payload: UpdateAdminUserPayload = {
        firstName: editForm.firstName,
        lastName: editForm.lastName,
        middleName: editForm.middleName,
        role: editForm.role,
        email: editForm.email,
        isActive: editForm.isActive,
      }

      if (editForm.password && editForm.password.trim()) {
        payload.password = editForm.password
      }

      await updateUser(editingUserId, payload)
      setEditOpen(false)
      showToast(t('users.updateSuccess'), 'success')
    } catch {
      const message = t('users.updateError')
      setError(message)
      showToast(message, 'error')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <>
      <div className="top-inline-row">
        <h2>{t('users.title')}</h2>
        <Button onClick={() => setCreateOpen(true)}>{t('users.add')}</Button>
      </div>

      {usersLoading ? <div className="page-loader">{t('users.loading')}</div> : null}
      {!usersLoading && usersError ? <p className="form-error">{usersError}</p> : null}

      {!usersLoading && !usersError && users.length === 0 ? (
        <div className="page-loader">{t('users.notFound')}</div>
      ) : null}

      {!usersLoading && !usersError && users.length > 0 ? (
        <Table
          columns={[
            {
              key: 'fullName',
              title: t('users.fullName'),
              render: (user: AdminUser) => user.fullName,
            },
            {
              key: 'role',
              title: t('users.role'),
              render: (user: AdminUser) => (user.role === 'Admin' ? t('users.roleAdmin') : t('users.roleManager')),
            },
            { key: 'email', title: t('users.email'), render: (user: AdminUser) => user.email },
            {
              key: 'active',
              title: t('users.active'),
              render: (user: AdminUser) => (
                <span className={`status-pill ${user.isActive ? 'st-ok' : 'st-err'}`}>
                  {user.isActive ? t('common.yes') : t('common.no')}
                </span>
              ),
            },
            {
              key: 'createdAt',
              title: t('users.createdAt'),
              render: (user: AdminUser) => new Date(user.createdAt).toLocaleString(locale),
            },
          ]}
          rows={users}
          rowKey={(user) => user.id}
          onRowClick={openEdit}
        />
      ) : null}

      <Modal open={createOpen} onClose={() => setCreateOpen(false)}>
        <h2 style={{ marginBottom: 20 }}>{t('users.createTitle')}</h2>
        <Input
          label={t('users.firstName')}
          value={createForm.firstName}
          onChange={(value) => setCreateForm((prev) => ({ ...prev, firstName: value }))}
          placeholder={t('users.firstName')}
        />
        <Input
          label={t('users.lastName')}
          value={createForm.lastName}
          onChange={(value) => setCreateForm((prev) => ({ ...prev, lastName: value }))}
          placeholder={t('users.lastName')}
        />
        <Input
          label={t('users.middleName')}
          value={createForm.middleName}
          onChange={(value) => setCreateForm((prev) => ({ ...prev, middleName: value }))}
          placeholder={t('users.middleName')}
        />
        <div className="input-group">
          <label>{t('users.role')}</label>
          <select
            value={createForm.role}
            onChange={(event) =>
              setCreateForm((prev) => ({ ...prev, role: event.target.value as UserRole }))
            }
          >
            <option value="Admin">{t('users.roleAdmin')}</option>
            <option value="Manager">{t('users.roleManager')}</option>
          </select>
        </div>
        <Input
          label={t('users.email')}
          type="email"
          value={createForm.email}
          onChange={(value) => setCreateForm((prev) => ({ ...prev, email: value }))}
          placeholder="admin@autoai.com"
        />
        <Input
          label={t('users.password')}
          type="password"
          value={createForm.password}
          onChange={(value) => setCreateForm((prev) => ({ ...prev, password: value }))}
          placeholder="••••••••"
        />

        <div className="modal-actions-row">
          <Button className="btn-flex" onClick={() => void create()} disabled={submitting}>
            {submitting ? t('users.creating') : t('users.createBtn')}
          </Button>
          <Button className="btn-flex" variant="outline" onClick={() => setCreateOpen(false)}>
            {t('common.cancel')}
          </Button>
        </div>

        {error ? <p className="form-error">{error}</p> : null}
      </Modal>

      <Modal open={editOpen} onClose={() => setEditOpen(false)}>
        <h2 style={{ marginBottom: 20 }}>{t('users.editTitle')}</h2>
        <Input
          label={t('users.firstName')}
          value={editForm.firstName}
          onChange={(value) => setEditForm((prev) => ({ ...prev, firstName: value }))}
          placeholder={t('users.firstName')}
        />
        <Input
          label={t('users.lastName')}
          value={editForm.lastName}
          onChange={(value) => setEditForm((prev) => ({ ...prev, lastName: value }))}
          placeholder={t('users.lastName')}
        />
        <Input
          label={t('users.middleName')}
          value={editForm.middleName}
          onChange={(value) => setEditForm((prev) => ({ ...prev, middleName: value }))}
          placeholder={t('users.middleName')}
        />
        <div className="input-group">
          <label>{t('users.role')}</label>
          <select
            value={editForm.role}
            onChange={(event) => setEditForm((prev) => ({ ...prev, role: event.target.value as UserRole }))}
          >
            <option value="Admin">{t('users.roleAdmin')}</option>
            <option value="Manager">{t('users.roleManager')}</option>
          </select>
        </div>
        <Input
          label={t('users.email')}
          type="email"
          value={editForm.email}
          onChange={(value) => setEditForm((prev) => ({ ...prev, email: value }))}
          placeholder="admin@autoai.com"
        />
        <Input
          label={t('users.newPasswordOptional')}
          type="password"
          value={editForm.password ?? ''}
          onChange={(value) => setEditForm((prev) => ({ ...prev, password: value }))}
          placeholder="••••••••"
        />

        <div className="input-group">
          <label>{t('users.active')}</label>
          <select
            value={String(editForm.isActive)}
            onChange={(event) =>
              setEditForm((prev) => ({ ...prev, isActive: event.target.value === 'true' }))
            }
          >
            <option value="true">{t('common.yes')}</option>
            <option value="false">{t('common.no')}</option>
          </select>
        </div>

        <div className="modal-actions-row">
          <Button className="btn-flex" onClick={() => void saveEdit()} disabled={submitting}>
            {submitting ? t('users.saving') : t('users.saveBtn')}
          </Button>
          <Button className="btn-flex" variant="outline" onClick={() => setEditOpen(false)}>
            {t('common.cancel')}
          </Button>
        </div>

        {error ? <p className="form-error">{error}</p> : null}
      </Modal>
    </>
  )
}
