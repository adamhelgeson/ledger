// ── API response wrapper ──────────────────────────────────────────────────────
export interface ApiResponse<T> {
  success: boolean
  data: T
  errors: string[]
}

// ── Enums (match C# server enums) ────────────────────────────────────────────
export type AccountType =
  | 'Checking'
  | 'Savings'
  | 'CreditCard'
  | 'Brokerage'
  | 'Retirement401k'

export type TransactionType = 'Debit' | 'Credit'
export type ImportStatus = 'Pending' | 'Complete' | 'Error'

// ── Account ───────────────────────────────────────────────────────────────────
export interface AccountDto {
  id: string
  name: string
  institution: string
  accountType: AccountType
  currency: string
  isActive: boolean
  currentBalance: number | null
  createdAt: string
  updatedAt: string
}

// ── Transaction ───────────────────────────────────────────────────────────────
export interface TransactionDto {
  id: string
  accountId: string
  accountName: string
  date: string
  description: string
  amount: number
  category: string
  transactionType: TransactionType
  notes: string | null
  importBatchId: string | null
  createdAt: string
}

export interface TransactionFilter {
  accountId?: string
  category?: string
  from?: string
  to?: string
  search?: string
  page?: number
  pageSize?: number
}

// ── Pagination ────────────────────────────────────────────────────────────────
export interface PaginatedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

// ── Dashboard ─────────────────────────────────────────────────────────────────
export interface CategorySpendingDto {
  category: string
  amount: number
  percentage: number
}

export interface AccountSummaryDto {
  id: string
  name: string
  institution: string
  accountType: string
  balance: number | null
  currency: string
}

export interface DashboardStatsDto {
  netWorth: number
  totalAssets: number
  totalLiabilities: number
  monthlySpending: number
  monthlySavingsRate: number
  spendingByCategory: CategorySpendingDto[]
  accountSummaries: AccountSummaryDto[]
}

// ── Holding ───────────────────────────────────────────────────────────────────
export interface HoldingDto {
  id: string
  accountId: string
  symbol: string
  name: string
  shares: number
  costBasis: number
  currentValue: number
  asOfDate: string
}

// ── Import / Bifrost ──────────────────────────────────────────────────────────
export interface ParsedTransactionRow {
  date: string
  description: string
  amount: number
  transactionType: TransactionType
  category: string
}

export interface ImportPreviewDto {
  filename: string
  detectedParser: string
  rowCount: number
  rows: ParsedTransactionRow[]
  errors: string[]
}

export interface ConfirmImportDto {
  importBatchId: string
  importedCount: number
  skippedCount: number
}

// ── Chat ──────────────────────────────────────────────────────────────────────
export interface ChatMessageRequest {
  message: string
  history?: { role: string; content: string }[]
}

export interface ChatResponseDto {
  message: string
  timestamp: string
}
