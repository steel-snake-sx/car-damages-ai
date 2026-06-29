import type { LoginResponse } from '../types/models'
import { httpRequest } from './http'

export async function loginApi(email: string, password: string): Promise<LoginResponse> {
  return await httpRequest<LoginResponse>('/api/auth/login', {
    method: 'POST',
    auth: false,
    body: { email, password },
  })
}
