import { config } from './config'
import type {
  AdjustPointsResult,
  AdminActivityReport,
  AdminAuditEntry,
  AdminDashboard,
  AdminDraw,
  AdminDrawDetail,
  AdminFraudSignal,
  AdminPointsReport,
  AdminPrize,
  AdminPrizesReport,
  AdminOperator,
  AdminOperatorsReport,
  AdminReward,
  AdminRewardsReport,
  AdminSession,
  AdminSponsor,
  AdminSponsorsReport,
  AdminTicketsReport,
  AdminUser,
  AdminUserDetail,
  AdminUserList,
  PlatformSettings,
  PrizeFulfillmentStatus,
  SaveDrawPayload,
  SaveOperatorPayload,
  SaveRewardPayload,
  SaveSponsorPayload,
  UserStatus,
} from './types'

/**
 * An expected API failure. `code` is the stable machine-readable value the backend attaches to its
 * RFC 7807 problem responses, so screens can branch on it without matching on message text.
 */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly code: string,
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }

  /** The caller is signed in but is not a back-office operator, or lacks the role for this action. */
  get isForbidden() {
    return this.status === 403
  }

  get isUnauthorized() {
    return this.status === 401
  }
}

/** Supplies the Authorization header. Set once at startup by the auth provider. */
let authorize: () => Promise<Record<string, string>> = async () => ({})

export function setAuthorizationProvider(provider: () => Promise<Record<string, string>>) {
  authorize = provider
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')

  if (init.body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  for (const [key, value] of Object.entries(await authorize())) {
    headers.set(key, value)
  }

  const response = await fetch(`${config.apiBaseUrl}${path}`, { ...init, headers })

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()
  const payload = text.length > 0 ? safeParse(text) : null

  if (!response.ok) {
    throw new ApiError(
      response.status,
      typeof payload?.code === 'string' ? payload.code : 'unexpected_error',
      typeof payload?.title === 'string' ? payload.title : `Request failed (${response.status}).`,
    )
  }

  return payload as T
}

// A proxy error or crash can return HTML where JSON was expected; surface that as a normal failure.
function safeParse(text: string): Record<string, unknown> | null {
  try {
    return JSON.parse(text)
  } catch {
    return null
  }
}

function query(params: Record<string, string | number | undefined | null>) {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') {
      search.set(key, String(value))
    }
  }
  const encoded = search.toString()
  return encoded.length > 0 ? `?${encoded}` : ''
}

const base = '/api/v1/admin'

export const api = {
  signIn: () => request<AdminSession>(`${base}/session`, { method: 'POST' }),

  dashboard: () => request<AdminDashboard>(`${base}/dashboard`),

  activity: (days: number) => request<AdminActivityReport>(`${base}/activity${query({ days })}`),

  points: (months: number) => request<AdminPointsReport>(`${base}/points${query({ months })}`),

  fraud: (take = 50) => request<AdminFraudSignal[]>(`${base}/fraud${query({ take })}`),

  audit: (take = 50) => request<AdminAuditEntry[]>(`${base}/audit${query({ take })}`),

  users: (params: { query?: string; status?: UserStatus | ''; page?: number; pageSize?: number }) =>
    request<AdminUserList>(`${base}/users${query(params)}`),

  user: (id: string, days = 7) => request<AdminUserDetail>(`${base}/users/${id}${query({ days })}`),

  adjustPoints: (id: string, amount: number, reason: string) =>
    request<AdjustPointsResult>(`${base}/users/${id}/points`, {
      method: 'POST',
      body: JSON.stringify({ amount, reason }),
    }),

  changeStatus: (id: string, status: UserStatus, reason?: string) =>
    request<AdminUser>(`${base}/users/${id}/status`, {
      method: 'POST',
      body: JSON.stringify({ status, reason: reason ?? null }),
    }),

  settings: () => request<PlatformSettings>(`${base}/settings`),

  saveSettings: (settings: PlatformSettings) =>
    request<PlatformSettings>(`${base}/settings`, {
      method: 'PUT',
      body: JSON.stringify(settings),
    }),

  draws: () => request<AdminDraw[]>(`${base}/draws`),

  draw: (id: string) => request<AdminDrawDetail>(`${base}/draws/${id}`),

  createDraw: (payload: SaveDrawPayload) =>
    request<AdminDraw>(`${base}/draws`, { method: 'POST', body: JSON.stringify(payload) }),

  updateDraw: (id: string, payload: SaveDrawPayload) =>
    request<AdminDraw>(`${base}/draws/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),

  changeDrawStatus: (id: string, status: string) =>
    request<AdminDraw>(`${base}/draws/${id}/status`, {
      method: 'POST',
      body: JSON.stringify({ status }),
    }),

  selectWinner: (id: string) => request<AdminDrawDetail>(`${base}/draws/${id}/winner`, { method: 'POST' }),

  tickets: (take = 50) => request<AdminTicketsReport>(`${base}/tickets${query({ take })}`),

  prizes: (take = 50) => request<AdminPrizesReport>(`${base}/prizes${query({ take })}`),

  updateFulfillment: (id: string, status: PrizeFulfillmentStatus, notes?: string | null) =>
    request<AdminPrize>(`${base}/prizes/${id}/fulfillment`, {
      method: 'POST',
      body: JSON.stringify({ status, notes: notes ?? null }),
    }),

  sponsors: () => request<AdminSponsorsReport>(`${base}/sponsors`),

  createSponsor: (payload: SaveSponsorPayload) =>
    request<AdminSponsor>(`${base}/sponsors`, { method: 'POST', body: JSON.stringify(payload) }),

  updateSponsor: (id: string, payload: SaveSponsorPayload) =>
    request<AdminSponsor>(`${base}/sponsors/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),

  rewards: () => request<AdminRewardsReport>(`${base}/rewards`),

  createReward: (payload: SaveRewardPayload) =>
    request<AdminReward>(`${base}/rewards`, { method: 'POST', body: JSON.stringify(payload) }),

  updateReward: (id: string, payload: SaveRewardPayload) =>
    request<AdminReward>(`${base}/rewards/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),

  operators: () => request<AdminOperatorsReport>(`${base}/operators`),

  createOperator: (payload: SaveOperatorPayload) =>
    request<AdminOperator>(`${base}/operators`, { method: 'POST', body: JSON.stringify(payload) }),

  updateOperator: (id: string, payload: SaveOperatorPayload) =>
    request<AdminOperator>(`${base}/operators/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
}
