import { useDashboardStats } from '@/hooks/useDashboard'
import { formatUsd, formatUsdCompact, formatPct } from '@/lib/formatters'
import { Skeleton } from '@/components/ui/skeleton'
import { ErrorState } from '@/components/ui/LoadingState'
import { TrendingUp, TrendingDown, Wallet } from 'lucide-react'

export function VaultOfAsgard() {
  const { data: stats, isLoading, isError } = useDashboardStats()

  if (isLoading) return <VaultSkeleton />
  if (isError || !stats) return <ErrorState />

  const savingsRate = stats.monthlySavingsRate

  return (
    <section className="card rune-corner p-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-1">
            Treasury Status
          </p>
          <h2 className="section-title text-xl">The Vault of Asgard</h2>
        </div>
        <div className="w-10 h-10 rounded-lg bg-gold/10 border border-gold/20 flex items-center justify-center">
          <Wallet size={18} className="text-gold" />
        </div>
      </div>

      <div className="divider-gold mb-6" />

      {/* Net worth hero */}
      <div className="flex flex-col items-center py-4 mb-6">
        <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-2">
          Total Net Worth
        </p>
        <p className="font-mono text-5xl font-bold tracking-tighter text-text-primary tabular-nums">
          {formatUsd(stats.netWorth)}
        </p>
        <p className="text-xs text-text-muted font-mono mt-1">
          As of today
        </p>
      </div>

      <div className="divider-gold mb-6" />

      {/* Assets / Liabilities / Savings grid */}
      <div className="grid grid-cols-3 gap-4">
        <StatTile
          label="Total Assets"
          value={formatUsdCompact(stats.totalAssets)}
          icon={<TrendingUp size={14} className="text-worthy" />}
          valueClass="text-worthy"
        />
        <StatTile
          label="Liabilities"
          value={formatUsdCompact(stats.totalLiabilities)}
          icon={<TrendingDown size={14} className="text-banished" />}
          valueClass="text-banished"
        />
        <StatTile
          label="Vibranium Reserves"
          value={`${formatPct(savingsRate)}`}
          sub="savings rate"
          icon={<span className="text-xs">💎</span>}
          valueClass={savingsRate >= 20 ? 'text-worthy' : savingsRate >= 10 ? 'text-unworthy' : 'text-banished'}
        />
      </div>

      {/* Monthly spending pill */}
      <div className="mt-4 flex items-center justify-between px-4 py-3 rounded-lg bg-bg-elevated border border-border">
        <span className="text-xs text-text-muted font-mono uppercase tracking-wider">
          Month-to-date spending
        </span>
        <span className="font-mono font-semibold text-sm text-text-primary tabular-nums">
          {formatUsd(stats.monthlySpending)}
        </span>
      </div>
    </section>
  )
}

function StatTile({
  label,
  value,
  sub,
  icon,
  valueClass,
}: {
  label: string
  value: string
  sub?: string
  icon: React.ReactNode
  valueClass: string
}) {
  return (
    <div className="flex flex-col items-center text-center px-2 py-3 rounded-lg bg-bg-elevated border border-border">
      <div className="flex items-center gap-1.5 mb-1.5">
        {icon}
        <span className="text-xs text-text-muted font-mono uppercase tracking-wide leading-none">
          {label}
        </span>
      </div>
      <span className={`font-mono font-bold text-lg tabular-nums ${valueClass}`}>{value}</span>
      {sub && <span className="text-xs text-text-muted mt-0.5">{sub}</span>}
    </div>
  )
}

function VaultSkeleton() {
  return (
    <div className="card p-6 space-y-5">
      <div className="flex justify-between items-start">
        <div className="space-y-2">
          <Skeleton className="h-3 w-24" />
          <Skeleton className="h-6 w-48" />
        </div>
        <Skeleton className="h-10 w-10 rounded-lg" />
      </div>
      <Skeleton className="h-px w-full" />
      <div className="flex flex-col items-center gap-2 py-2">
        <Skeleton className="h-3 w-24" />
        <Skeleton className="h-12 w-64" />
      </div>
      <Skeleton className="h-px w-full" />
      <div className="grid grid-cols-3 gap-4">
        {[0, 1, 2].map((i) => (
          <Skeleton key={i} className="h-20 rounded-lg" />
        ))}
      </div>
    </div>
  )
}
