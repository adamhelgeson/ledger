// ── Currency formatting ───────────────────────────────────────────────────────
// All monetary values: USD, 2 decimal places, comma separators.
// Always rendered in JetBrains Mono via the .amount CSS class.

const usdFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const usdCompactFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 0,
  maximumFractionDigits: 0,
  notation: 'compact',
  compactDisplay: 'short',
})

/** Full USD: $1,234.56 */
export function formatUsd(value: number | null | undefined): string {
  if (value == null) return '—'
  return usdFormatter.format(value)
}

/** Compact USD for large numbers: $87.3K */
export function formatUsdCompact(value: number | null | undefined): string {
  if (value == null) return '—'
  return usdCompactFormatter.format(value)
}

/** Sign-prefixed: +$1,234.56 or -$1,234.56 */
export function formatUsdSigned(value: number): string {
  const formatted = usdFormatter.format(Math.abs(value))
  return value >= 0 ? `+${formatted}` : `-${formatted}`
}

/** Plain number with commas: 1,234.56 (no $ sign) */
export function formatNumber(value: number, decimals = 2): string {
  return value.toLocaleString('en-US', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  })
}

// ── Date formatting ───────────────────────────────────────────────────────────
const dateFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: 'numeric',
  year: 'numeric',
})

const dateShortFormatter = new Intl.DateTimeFormat('en-US', {
  month: 'short',
  day: 'numeric',
})

const dateIsoFormatter = new Intl.DateTimeFormat('en-CA') // yyyy-mm-dd

export function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return '—'
  try {
    return dateFormatter.format(new Date(dateStr))
  } catch {
    return dateStr
  }
}

export function formatDateShort(dateStr: string | null | undefined): string {
  if (!dateStr) return '—'
  try {
    return dateShortFormatter.format(new Date(dateStr))
  } catch {
    return dateStr
  }
}

export function formatDateIso(date: Date): string {
  return dateIsoFormatter.format(date)
}

// ── Percentage ────────────────────────────────────────────────────────────────
export function formatPct(value: number, decimals = 1): string {
  return `${value.toFixed(decimals)}%`
}

// ── Credit card worthiness ────────────────────────────────────────────────────
export type WorthinessLevel = 'worthy' | 'unworthy' | 'banished'

/** Returns worthiness level based on utilization percentage (0-100). */
export function getWorthiness(utilizationPct: number): WorthinessLevel {
  if (utilizationPct < 30) return 'worthy'
  if (utilizationPct < 70) return 'unworthy'
  return 'banished'
}

export const WORTHINESS_LABELS: Record<WorthinessLevel, string> = {
  worthy: 'Worthy ⚡',
  unworthy: 'Unworthy 🔨',
  banished: 'Banished 💀',
}

export const WORTHINESS_COLORS: Record<WorthinessLevel, string> = {
  worthy: 'text-worthy border-worthy/40 bg-worthy/10',
  unworthy: 'text-unworthy border-unworthy/40 bg-unworthy/10',
  banished: 'text-banished border-banished/40 bg-banished/10',
}

// ── Account type helpers ──────────────────────────────────────────────────────
import type { AccountType } from '@/types/api'

export const ACCOUNT_TYPE_LABELS: Record<AccountType, string> = {
  Checking: 'Checking',
  Savings: 'Savings',
  CreditCard: 'Credit Card',
  Brokerage: 'Brokerage',
  Retirement401k: '401(k)',
}

export const ACCOUNT_TYPE_ICONS: Record<AccountType, string> = {
  Checking: '🏦',
  Savings: '💎',
  CreditCard: '⚡',
  Brokerage: '📈',
  Retirement401k: '🔮',
}

/** Returns whether this account type is a liability (credit card). */
export function isLiability(type: AccountType): boolean {
  return type === 'CreditCard'
}

/** Assumed credit limit per card (placeholder until we track real limits). */
export const ASSUMED_CREDIT_LIMIT = 5_000
