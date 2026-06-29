import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import type {
  AdminRequestQuery,
  AdminUser,
  AuthUser,
  CreateAdminUserPayload,
  DamageRequest,
  LoginResponse,
  UpdateAdminUserPayload,
  UpdateDamageRequestPayload,
} from '../types/models'
import {
  clearStoredToken,
  clearStoredUser,
  getStoredToken,
  getStoredUser,
  setStoredToken,
  setStoredUser,
} from './authStorage'
import { loginApi } from './authApi'
import {
  approveDamageRequestApi,
  exportAdminRequestsExcelApi,
  getAdminRequestByIdApi,
  getAdminRequestsApi,
  reanalyzeDamageRequestApi,
  rejectDamageRequestApi,
  updateDamageRequestApi,
} from './requestsApi'
import { createAdminUserApi, getAdminUsersApi, updateAdminUserApi } from './usersApi'
import { getCurrentLanguage } from './i18n'
import { en } from '../locales/en'
import { ru } from '../locales/ru'
import type { LocaleDictionary } from '../locales/types'

type AppContextValue = {
  authUser: AuthUser | null
  token: string | null
  requests: DamageRequest[]
  users: AdminUser[]
  authLoading: boolean
  requestsLoading: boolean
  usersLoading: boolean
  requestsError: string
  usersError: string
  currentRequestQuery: AdminRequestQuery
  login: (email: string, password: string) => Promise<boolean>
  logout: () => void
  setRequestsQuery: (query: AdminRequestQuery) => void
  refreshRequests: () => Promise<void>
  refreshUsers: () => Promise<void>
  getRequestById: (id: string) => Promise<DamageRequest>
  updateRequest: (id: string, payload: UpdateDamageRequestPayload) => Promise<void>
  approveRequest: (id: string) => Promise<void>
  rejectRequest: (id: string) => Promise<void>
  reanalyzeRequest: (id: string) => Promise<void>
  exportRequestsExcel: () => Promise<void>
  createUser: (payload: CreateAdminUserPayload) => Promise<void>
  updateUser: (id: string, payload: UpdateAdminUserPayload) => Promise<void>
}

const AppContext = createContext<AppContextValue | undefined>(undefined)

type AppProviderProps = {
  children: React.ReactNode
}

export function AppProvider({ children }: AppProviderProps) {
  const [authUser, setAuthUser] = useState<AuthUser | null>(null)
  const [token, setToken] = useState<string | null>(null)
  const [requests, setRequests] = useState<DamageRequest[]>([])
  const [users, setUsers] = useState<AdminUser[]>([])
  const [authLoading, setAuthLoading] = useState(true)
  const [requestsLoading, setRequestsLoading] = useState(false)
  const [usersLoading, setUsersLoading] = useState(false)
  const [requestsError, setRequestsError] = useState('')
  const [usersError, setUsersError] = useState('')
  const [currentRequestQuery, setCurrentRequestQuery] = useState<AdminRequestQuery>({
    sortBy: 'createdAt',
    sortDirection: 'desc',
    search: '',
  })

  const isAdmin = authUser?.role === 'Admin'

  useEffect(() => {
    const storedToken = getStoredToken()
    const storedUser = getStoredUser()

    if (!storedToken || !storedUser || isTokenExpired(storedToken)) {
      clearStoredToken()
      clearStoredUser()
      clearLocalStorage()
      setToken(null)
      setAuthUser(null)
      setAuthLoading(false)
      return
    }

    setToken(storedToken)
    setAuthUser(storedUser)
    setAuthLoading(false)
  }, [])

  const logout = useCallback(() => {
    clearStoredToken()
    clearStoredUser()
    clearLocalStorage()
    setToken(null)
    setAuthUser(null)
    setRequests([])
    setUsers([])
    setRequestsError('')
    setUsersError('')
  }, [])

  const refreshRequests = useCallback(async () => {
    if (!token || !authUser) {
      setRequests([])
      setRequestsError('')
      return
    }

    if (isTokenExpired(token)) {
      logout()
      return
    }

    setRequestsLoading(true)
    setRequestsError('')
    try {
      const response = await getAdminRequestsApi(currentRequestQuery)
      setRequests(response)
    } catch (error) {
      setRequests([])
      const message = error instanceof Error ? error.message : fallbackText('requests.loadingError')
      setRequestsError(message)
    } finally {
      setRequestsLoading(false)
    }
  }, [authUser, currentRequestQuery, logout, token])

  const refreshUsers = useCallback(async () => {
    if (!token || !isAdmin) {
      setUsers([])
      setUsersError('')
      return
    }

    if (isTokenExpired(token)) {
      logout()
      return
    }

    setUsersLoading(true)
    setUsersError('')
    try {
      const response = await getAdminUsersApi()
      setUsers(response)
    } catch (error) {
      setUsers([])
      const message = error instanceof Error ? error.message : fallbackText('users.loadingError')
      setUsersError(message)
    } finally {
      setUsersLoading(false)
    }
  }, [isAdmin, logout, token])

  useEffect(() => {
    if (!token) {
      return
    }

    const expiresAtMs = getTokenExpiryMs(token)
    if (!expiresAtMs) {
      return
    }

    const remainingMs = expiresAtMs - Date.now()
    if (remainingMs <= 0) {
      logout()
      return
    }

    const timeoutId = window.setTimeout(() => {
      logout()
    }, remainingMs)

    return () => {
      window.clearTimeout(timeoutId)
    }
  }, [logout, token])

  useEffect(() => {
    if (!token || !authUser) {
      setRequests([])
      setUsers([])
      setRequestsError('')
      setUsersError('')
      return
    }

    void refreshRequests()
    if (authUser.role === 'Admin') {
      void refreshUsers()
    } else {
      setUsers([])
      setUsersError('')
    }
  }, [authUser, refreshRequests, refreshUsers, token])

  const login = useCallback(async (email: string, password: string) => {
    let response: LoginResponse

    try {
      response = await loginApi(email, password)
    } catch {
      return false
    }

    const nextToken = response.accessToken
    if (isTokenExpired(nextToken)) {
      clearStoredToken()
      clearStoredUser()
      clearLocalStorage()
      setToken(null)
      setAuthUser(null)
      return false
    }

    const nextUser: AuthUser = {
      email,
      role: response.role,
    }

    setStoredToken(nextToken)
    setStoredUser(nextUser)

    setToken(nextToken)
    setAuthUser(nextUser)
    return true
  }, [])

  const getRequestById = useCallback(async (id: string) => {
    const response = await getAdminRequestByIdApi(id)

    setRequests((prev) => {
      const without = prev.filter((request) => request.id !== response.id)
      return [response, ...without]
    })

    return response
  }, [])

  const updateRequest = useCallback(
    async (id: string, payload: UpdateDamageRequestPayload) => {
      await updateDamageRequestApi(id, payload)
      await refreshRequests()
    },
    [refreshRequests],
  )

  const approveRequest = useCallback(
    async (id: string) => {
      await approveDamageRequestApi(id)

      await refreshRequests()
    },
    [refreshRequests],
  )

  const rejectRequest = useCallback(
    async (id: string) => {
      await rejectDamageRequestApi(id)
      await refreshRequests()
    },
    [refreshRequests],
  )

  const createUser = useCallback(
    async (payload: CreateAdminUserPayload) => {
      await createAdminUserApi(payload)
      await refreshUsers()
    },
    [refreshUsers],
  )

  const reanalyzeRequest = useCallback(
    async (id: string) => {
      await reanalyzeDamageRequestApi(id)
      await refreshRequests()
    },
    [refreshRequests],
  )

  const exportRequestsExcel = useCallback(async () => {
    await exportAdminRequestsExcelApi()
  }, [])

  const updateUser = useCallback(
    async (id: string, payload: UpdateAdminUserPayload) => {
      await updateAdminUserApi(id, payload)
      await refreshUsers()
    },
    [refreshUsers],
  )

  const setRequestsQuery = useCallback((query: AdminRequestQuery) => {
    setCurrentRequestQuery((prev) => ({ ...prev, ...query }))
  }, [])

  const value = useMemo<AppContextValue>(
    () => ({
      authUser,
      token,
      requests,
      users,
      authLoading,
      requestsLoading,
      usersLoading,
      requestsError,
      usersError,
      currentRequestQuery,
      login,
      logout,
      setRequestsQuery,
      refreshRequests,
      refreshUsers,
      getRequestById,
      updateRequest,
      approveRequest,
      rejectRequest,
      reanalyzeRequest,
      exportRequestsExcel,
      createUser,
      updateUser,
    }),
    [
      authLoading,
      authUser,
      approveRequest,
      createUser,
      currentRequestQuery,
      getRequestById,
      login,
      logout,
      refreshRequests,
      refreshUsers,
      rejectRequest,
      reanalyzeRequest,
      exportRequestsExcel,
      requests,
      requestsLoading,
      setRequestsQuery,
      token,
      updateRequest,
      updateUser,
      users,
      usersLoading,
      requestsError,
      usersError,
    ],
  )

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>
}

export function useAppContext() {
  const context = useContext(AppContext)
  if (!context) {
    throw new Error('useAppContext must be used inside AppProvider')
  }

  return context
}

function getTokenExpiryMs(jwtToken: string): number | null {
  const parts = jwtToken.split('.')
  if (parts.length < 2) {
    return null
  }

  const base64Url = parts[1]
  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
  const padding = base64.length % 4
  const normalized = padding === 0 ? base64 : `${base64}${'='.repeat(4 - padding)}`

  try {
    const payload = JSON.parse(atob(normalized)) as { exp?: unknown }
    if (typeof payload.exp !== 'number' || !Number.isFinite(payload.exp)) {
      return null
    }

    return payload.exp * 1000
  } catch {
    return null
  }
}

function isTokenExpired(jwtToken: string): boolean {
  const expiresAtMs = getTokenExpiryMs(jwtToken)
  if (!expiresAtMs) {
    return false
  }

  return Date.now() >= expiresAtMs
}

function clearLocalStorage() {
  if (typeof window === 'undefined') {
    return
  }

  try {
    window.localStorage.clear()
  } catch {
  }
}

function fallbackText(key: string): string {
  const lang = getCurrentLanguage()
  const dictionary: LocaleDictionary = lang === 'en' ? en : ru
  const parts = key.split('.')

  let current: unknown = dictionary
  for (const part of parts) {
    if (!current || typeof current !== 'object') {
      return key
    }

    current = (current as Record<string, unknown>)[part]
  }

  return typeof current === 'string' ? current : key
}
