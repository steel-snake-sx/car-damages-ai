export type RequestStatus = 'New' | 'AiProcessed' | 'Approved' | 'Rejected' | 'Notified'

export type UserRole = 'Admin' | 'Manager'

export interface LoginResponse {
  accessToken: string
  tokenType: string
  expiresAtUtc: string
  role: UserRole
}

export interface AuthUser {
  email: string
  role: UserRole
}

export interface DamageRequestPhoto {
  id: string
  fileName: string
  filePath: string
  sortOrder: number
  createdAt: string
}

export interface DamageEstimateItem {
  id: string
  partName: string
  damageDescription: string
  severity: string
  estimatedCost: number
  confidence: number
}

export interface RequestNotification {
  id: string
  recipientEmail: string
  notificationType: string
  subject: string | null
  status: string
  sentAt: string | null
  errorMessage: string | null
  createdAt: string
}

export interface DamageRequest {
  id: string
  createdAt: string
  updatedAt: string
  status: RequestStatus
  firstName: string
  lastName: string
  middleName: string | null
  fullName: string
  email: string
  phone: string
  carBrand: string
  carModel: string
  carYear: number
  aiIsCar: boolean
  aiSummary: string
  aiEstimatedTotalCost: number
  adminDecisionComment: string | null
  approvedByUserId: string | null
  approvedByFullName: string | null
  photos: DamageRequestPhoto[]
  estimateItems: DamageEstimateItem[]
  notifications: RequestNotification[]
}

export interface CreateDamageRequestPayload {
  firstName: string
  lastName: string
  middleName: string
  email: string
  phone: string
  carBrand: string
  carModel: string
  carYear: number
  files: File[]
}

export interface CreateDamageRequestResponse {
  id: string
  status: RequestStatus
  createdAt: string
}

export interface UpdateDamageRequestPayload {
  firstName: string
  lastName: string
  middleName: string
  email: string
  phone: string
  carBrand: string
  carModel: string
  carYear: number
  status: RequestStatus
  adminDecisionComment: string
}

export interface UpdateDamageRequestResponse {
  id: string
  status: RequestStatus
  updatedAt: string
}

export interface ApproveDamageRequestResponse {
  id: string
  status: RequestStatus
  approvedByUserId: string | null
  notificationStatus: string
  updatedAt: string
}

export interface RejectDamageRequestResponse {
  id: string
  status: RequestStatus
  updatedAt: string
}

export interface AdminRequestQuery {
  search?: string
  sortBy?: 'createdAt' | 'status' | 'customer' | 'car'
  sortDirection?: 'asc' | 'desc'
}

export interface AdminUser {
  id: string
  firstName: string
  middleName: string | null
  lastName: string
  fullName: string
  email: string
  role: UserRole
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface EmailHistoryItem {
  id: string
  requestId: string
  recipientEmail: string
  subject: string | null
  status: string
  sentAt?: string | null
  errorMessage?: string | null
  createdAt: string
  fullName: string
}

export interface CreateAdminUserPayload {
  firstName: string
  middleName: string
  lastName: string
  email: string
  password: string
  role: UserRole
  isActive: boolean
}

export interface UpdateAdminUserPayload {
  firstName: string
  middleName: string
  lastName: string
  email: string
  role: UserRole
  isActive: boolean
  password?: string
}
