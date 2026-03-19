import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts'
import { useDashboardStats } from '@/hooks/useDashboard'
import { formatUsd, formatUsdCompact } from '@/lib/formatters'
import { Skeleton } from '@/components/ui/skeleton'
import { ErrorState } from '@/components/ui/LoadingState'
import type { CategorySpendingDto } from '@/types/api'

// Gold → orange gradient palette for bars
const BAR_COLORS = [
  '#D4A84B', '#C9973A', '#BE8729', '#E8BF6A', '#F0C878',
  '#4C9AFF', '#6BAEFF', '#3A80E0', '#A78BFA', '#F472B6',
]

interface CustomTooltipProps {
  active?: boolean
  payload?: { value: number; payload: CategorySpendingDto }[]
  label?: string
}

function CustomTooltip({ active, payload, label }: CustomTooltipProps) {
  if (!active || !payload?.length) return null
  const item = payload[0]
  return (
    <div className="card border-gold/30 px-3 py-2 text-sm shadow-gold-glow">
      <p className="font-display text-gold text-xs uppercase tracking-wider mb-1">{label}</p>
      <p className="font-mono font-bold text-text-primary tabular-nums">
        {formatUsd(item.value)}
      </p>
      <p className="font-mono text-text-muted text-xs">
        {item.payload.percentage.toFixed(1)}% of spending
      </p>
    </div>
  )
}

export function RagnarokReport() {
  const { data: stats, isLoading, isError } = useDashboardStats()

  if (isLoading) return <RagnarokSkeleton />
  if (isError || !stats) return <ErrorState />

  const data = [...stats.spendingByCategory]
    .sort((a, b) => b.amount - a.amount)
    .slice(0, 10) // top 10 categories

  return (
    <section className="card p-6 animate-fade-in">
      <div className="flex items-center justify-between mb-2">
        <div>
          <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-1">
            This Month
          </p>
          <h2 className="section-title text-lg">Ragnarök Report</h2>
        </div>
        <div className="text-right">
          <p className="text-xs text-text-muted font-mono">Total</p>
          <p className="font-mono font-bold text-gold tabular-nums">
            {formatUsd(stats.monthlySpending)}
          </p>
        </div>
      </div>

      <p className="text-xs text-text-muted mb-4 font-mono">
        How much you've destroyed your budget this cycle.
      </p>

      <div className="divider-gold mb-5" />

      {data.length === 0 ? (
        <p className="text-sm text-text-muted text-center py-8 font-mono">
          Even Thor had slow months.
        </p>
      ) : (
        <ResponsiveContainer width="100%" height={260}>
          <BarChart
            data={data}
            margin={{ top: 4, right: 4, left: -10, bottom: 4 }}
            barCategoryGap="28%"
          >
            <CartesianGrid
              strokeDasharray="3 3"
              stroke="#2A2F4540"
              vertical={false}
            />
            <XAxis
              dataKey="category"
              tick={{ fontSize: 10, fill: '#8892A4', fontFamily: 'JetBrains Mono' }}
              tickLine={false}
              axisLine={false}
              interval={0}
              angle={-35}
              textAnchor="end"
              height={52}
            />
            <YAxis
              tickFormatter={(v: number) => formatUsdCompact(v)}
              tick={{ fontSize: 10, fill: '#8892A4', fontFamily: 'JetBrains Mono' }}
              tickLine={false}
              axisLine={false}
              width={52}
            />
            <Tooltip content={<CustomTooltip />} cursor={{ fill: '#D4A84B08' }} />
            <Bar dataKey="amount" radius={[3, 3, 0, 0]} maxBarSize={40}>
              {data.map((entry, index) => (
                <Cell
                  key={entry.category}
                  fill={BAR_COLORS[index % BAR_COLORS.length]}
                  fillOpacity={0.85}
                />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      )}
    </section>
  )
}

function RagnarokSkeleton() {
  return (
    <div className="card p-6 space-y-5">
      <div className="flex justify-between">
        <div className="space-y-2">
          <Skeleton className="h-3 w-20" />
          <Skeleton className="h-6 w-44" />
        </div>
        <Skeleton className="h-10 w-20" />
      </div>
      <Skeleton className="h-px w-full" />
      <Skeleton className="h-[260px] w-full rounded-lg" />
    </div>
  )
}
