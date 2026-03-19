import type { AccountDto } from '@/types/api'
import {
  formatUsd,
  ACCOUNT_TYPE_LABELS,
  ACCOUNT_TYPE_ICONS,
  isLiability,
  getWorthiness,
  WORTHINESS_LABELS,
  WORTHINESS_COLORS,
  ASSUMED_CREDIT_LIMIT,
} from '@/lib/formatters'
import { BalanceSparkline } from './BalanceSparkline'
import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import { useDashboardStore } from '@/stores/dashboardStore'

interface AccountCardProps {
  account: AccountDto
}

export function AccountCard({ account }: AccountCardProps) {
  const setSelectedAccountId = useDashboardStore((s) => s.setSelectedAccountId)
  const selectedAccountId = useDashboardStore((s) => s.selectedAccountId)
  const isSelected = selectedAccountId === account.id

  const balance = account.currentBalance ?? 0
  const liability = isLiability(account.accountType)
  const isPositive = !liability

  // Credit card worthiness
  const utilization = liability ? (balance / ASSUMED_CREDIT_LIMIT) * 100 : 0
  const worthiness = liability ? getWorthiness(utilization) : null

  return (
    <button
      onClick={() =>
        setSelectedAccountId(isSelected ? null : account.id)
      }
      className={cn(
        'card card-hover text-left w-full p-5 group',
        'transition-all duration-300',
        isSelected && 'border-gold/50 shadow-gold-glow',
      )}
    >
      {/* Top row: icon + type badge */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-2">
          <span className="text-lg leading-none">
            {ACCOUNT_TYPE_ICONS[account.accountType]}
          </span>
          <div>
            <p className="text-xs font-mono text-text-muted uppercase tracking-wider leading-none mb-0.5">
              {account.institution}
            </p>
            <p className="text-xs text-text-secondary">
              {ACCOUNT_TYPE_LABELS[account.accountType]}
            </p>
          </div>
        </div>
        {worthiness && (
          <span className={cn('text-xs font-mono border rounded px-1.5 py-0.5', WORTHINESS_COLORS[worthiness])}>
            {WORTHINESS_LABELS[worthiness]}
          </span>
        )}
      </div>

      {/* Account name */}
      <h3 className="font-display text-sm font-semibold text-text-primary tracking-wide mb-3 leading-tight">
        {account.name}
      </h3>

      {/* Sparkline + balance row */}
      <div className="flex items-end justify-between gap-3">
        <div className="flex-1 flex items-end">
          <BalanceSparkline
            balance={balance}
            accountId={account.id}
            positive={isPositive}
            width={100}
            height={36}
          />
        </div>
        <div className="text-right">
          <p
            className={cn(
              'font-mono font-bold tabular-nums tracking-tight text-lg',
              liability ? 'text-banished' : 'text-text-primary',
            )}
          >
            {formatUsd(balance)}
          </p>
          {liability && (
            <p className="text-xs font-mono text-text-muted mt-0.5">
              {utilization.toFixed(0)}% utilized
            </p>
          )}
        </div>
      </div>

      {/* Bottom: currency + active indicator */}
      <div className="flex items-center justify-between mt-3 pt-3 border-t border-border">
        <Badge variant="default" className="text-xs">
          {account.currency}
        </Badge>
        <div
          className={cn(
            'w-1.5 h-1.5 rounded-full',
            account.isActive ? 'bg-worthy' : 'bg-text-muted',
          )}
        />
      </div>
    </button>
  )
}
