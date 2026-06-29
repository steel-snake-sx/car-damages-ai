import type {
  AdminRequestQuery,
  ApproveDamageRequestResponse,
  CreateDamageRequestPayload,
  CreateDamageRequestResponse,
  DamageRequest,
  EmailHistoryItem,
  RejectDamageRequestResponse,
  UpdateDamageRequestPayload,
  UpdateDamageRequestResponse,
} from '../types/models'
import { httpDownload, httpRequest } from './http'

type AdminRequestsApiResponse = DamageRequest[] | { items?: DamageRequest[] }

export async function createDamageRequestApi(
  payload: CreateDamageRequestPayload,
): Promise<CreateDamageRequestResponse> {
  const formData = new FormData()
  formData.append('firstName', payload.firstName)
  formData.append('lastName', payload.lastName)
  formData.append('middleName', payload.middleName)
  formData.append('email', payload.email)
  formData.append('phone', payload.phone)
  formData.append('carBrand', payload.carBrand)
  formData.append('carModel', payload.carModel)
  formData.append('carYear', String(payload.carYear))

  for (const file of payload.files) {
    formData.append('files', file)
  }

  return await httpRequest<CreateDamageRequestResponse>('/api/requests', {
    method: 'POST',
    auth: false,
    formData: true,
    body: formData,
  })
}

export async function getAdminRequestsApi(query: AdminRequestQuery = {}): Promise<DamageRequest[]> {
  const params = new URLSearchParams()

  if (query.search && query.search.trim()) {
    params.set('search', query.search.trim())
  }
  if (query.sortBy) {
    params.set('sortBy', query.sortBy)
  }
  if (query.sortDirection) {
    params.set('sortDirection', query.sortDirection)
  }

  const suffix = params.toString() ? `?${params.toString()}` : ''
  const response = await httpRequest<AdminRequestsApiResponse>(`/api/admin/requests${suffix}`)

  if (Array.isArray(response)) {
    return response
  }

  if (response && typeof response === 'object' && Array.isArray(response.items)) {
    return response.items
  }

  return []
}

export async function getAdminRequestByIdApi(id: string): Promise<DamageRequest> {
  return await httpRequest<DamageRequest>(`/api/admin/requests/${id}`)
}

export async function getEmailHistoryApi(): Promise<EmailHistoryItem[]> {
  return await httpRequest<EmailHistoryItem[]>('/api/admin/requests/notifications/history')
}

export async function resendEmailApi(id: string): Promise<void> {
  await httpRequest(`/api/admin/emails/${id}/resend`, {
    method: 'POST',
  })
}

export async function approveDamageRequestApi(id: string): Promise<ApproveDamageRequestResponse> {
  return await httpRequest<ApproveDamageRequestResponse>(`/api/admin/requests/${id}/approve`, {
    method: 'POST',
  })
}

export async function rejectDamageRequestApi(id: string): Promise<RejectDamageRequestResponse> {
  return await httpRequest<RejectDamageRequestResponse>(`/api/admin/requests/${id}/reject`, {
    method: 'POST',
  })
}

export async function updateDamageRequestApi(
  id: string,
  payload: UpdateDamageRequestPayload,
): Promise<UpdateDamageRequestResponse> {
  return await httpRequest<UpdateDamageRequestResponse>(`/api/admin/requests/${id}`, {
    method: 'PUT',
    body: payload,
  })
}

export async function reanalyzeDamageRequestApi(id: string): Promise<void> {
  await httpRequest(`/api/admin/requests/${id}/reanalyze`, {
    method: 'POST',
  })
}

export async function exportDamageRequestDocApi(id: string): Promise<void> {
  const { blob, filename } = await httpDownload(`/api/admin/requests/${id}/export`)
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename.toLowerCase().endsWith('.docx') ? filename : 'damage-report.docx'
  document.body.appendChild(link)
  link.click()
  link.remove()
  window.URL.revokeObjectURL(url)
}

export async function exportAdminRequestsExcelApi(): Promise<void> {
  const { blob, filename } = await httpDownload('/api/admin/requests/export')
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename.toLowerCase().endsWith('.xlsx') ? filename : 'requests_export.xlsx'
  document.body.appendChild(link)
  link.click()
  link.remove()
  window.URL.revokeObjectURL(url)
}
