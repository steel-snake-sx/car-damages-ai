import type { AdminUser, CreateAdminUserPayload, UpdateAdminUserPayload } from '../types/models'
import { httpRequest } from './http'

export async function getAdminUsersApi(): Promise<AdminUser[]> {
  return await httpRequest<AdminUser[]>('/api/admin/users')
}

export async function createAdminUserApi(payload: CreateAdminUserPayload): Promise<{ id: string }> {
  return await httpRequest<{ id: string }>('/api/admin/users', {
    method: 'POST',
    body: payload,
  })
}

export async function updateAdminUserApi(
  id: string,
  payload: UpdateAdminUserPayload,
): Promise<{ id: string; updatedAt: string }> {
  return await httpRequest<{ id: string; updatedAt: string }>(`/api/admin/users/${id}`, {
    method: 'PUT',
    body: payload,
  })
}
