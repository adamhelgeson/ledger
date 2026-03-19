import type {
  AccountDto,
  ApiResponse,
  ConfirmImportDto,
  DashboardStatsDto,
  ImportPreviewDto,
  PaginatedResult,
  ParsedTransactionRow,
  TransactionDto,
  TransactionFilter,
  ChatMessageRequest,
  ChatResponseDto,
} from '@/types/api'

const BASE = '/api'

class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly errors: string[],
    message: string,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  })
  const json: ApiResponse<T> = await res.json()
  if (!json.success) {
    throw new ApiError(res.status, json.errors, json.errors.join(', '))
  }
  return json.data
}

// ── Accounts ──────────────────────────────────────────────────────────────────
export const accountsApi = {
  getAll: () => request<AccountDto[]>('/accounts'),
  getById: (id: string) => request<AccountDto>(`/accounts/${id}`),
  create: (body: unknown) =>
    request<AccountDto>('/accounts', { method: 'POST', body: JSON.stringify(body) }),
  update: (id: string, body: unknown) =>
    request<AccountDto>(`/accounts/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (id: string) => request<void>(`/accounts/${id}`, { method: 'DELETE' }),
}

// ── Transactions ──────────────────────────────────────────────────────────────
export const transactionsApi = {
  getAll: (filter: TransactionFilter = {}) => {
    const params = new URLSearchParams()
    if (filter.accountId) params.set('accountId', filter.accountId)
    if (filter.category) params.set('category', filter.category)
    if (filter.from) params.set('from', filter.from)
    if (filter.to) params.set('to', filter.to)
    if (filter.search) params.set('search', filter.search)
    if (filter.page) params.set('page', String(filter.page))
    if (filter.pageSize) params.set('pageSize', String(filter.pageSize))
    const qs = params.toString()
    return request<PaginatedResult<TransactionDto>>(`/transactions${qs ? `?${qs}` : ''}`)
  },
}

// ── Dashboard ─────────────────────────────────────────────────────────────────
export const dashboardApi = {
  getStats: () => request<DashboardStatsDto>('/dashboard/stats'),
}

// ── Import / Bifrost ──────────────────────────────────────────────────────────
// Preview uses multipart/form-data — bypass the default JSON Content-Type header.
export async function previewImport(accountId: string, file: File): Promise<ImportPreviewDto> {
  const form = new FormData()
  form.append('file', file)
  const res = await fetch(`${BASE}/import/preview?accountId=${accountId}`, {
    method: 'POST',
    body: form,
    // No Content-Type header — browser sets multipart/form-data with boundary
  })
  const json: ApiResponse<ImportPreviewDto> = await res.json()
  if (!json.success) throw new ApiError(res.status, json.errors, json.errors.join(', '))
  return json.data
}

export function confirmImport(
  accountId: string,
  filename: string,
  rows: ParsedTransactionRow[],
): Promise<ConfirmImportDto> {
  return request<ConfirmImportDto>('/import/confirm', {
    method: 'POST',
    body: JSON.stringify({ accountId, filename, rows }),
  })
}

// ── Chat ──────────────────────────────────────────────────────────────────────
export const chatApi = {
  send: (body: ChatMessageRequest) =>
    request<ChatResponseDto>('/chat', { method: 'POST', body: JSON.stringify(body) }),
}

export async function sendChatMessage(message: string): Promise<string> {
  const response = await chatApi.send({ message })
  return response.message
}

export { ApiError }
