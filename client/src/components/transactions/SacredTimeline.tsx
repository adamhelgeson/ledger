import { useState } from 'react'
import { ChevronUp, ChevronDown, ChevronLeft, ChevronRight } from 'lucide-react'
import { useTransactions } from '@/hooks/useTransactions'
import { useDashboardStore } from '@/stores/dashboardStore'
import { TransactionFilters } from './TransactionFilters'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { EmptyState, ErrorState } from '@/components/ui/LoadingState'
import { cn, getCategoryColor } from '@/lib/utils'
import { formatUsd, formatDateShort } from '@/lib/formatters'
import type { TransactionDto } from '@/types/api'

type SortField = 'date' | 'amount' | 'description' | 'category'
type SortDir = 'asc' | 'desc'

export function SacredTimeline() {
  const { filter, setFilter } = useDashboardStore()
  const { data, isLoading, isError, isFetching } = useTransactions(filter)
  const [sort, setSort] = useState<{ field: SortField; dir: SortDir }>({
    field: 'date',
    dir: 'desc',
  })

  const handleSort = (field: SortField) => {
    setSort((s) => ({ field, dir: s.field === field && s.dir === 'desc' ? 'asc' : 'desc' }))
  }

  const sortedItems = data
    ? [...data.items].sort((a, b) => {
        let cmp = 0
        if (sort.field === 'date') cmp = a.date.localeCompare(b.date)
        else if (sort.field === 'amount') cmp = a.amount - b.amount
        else if (sort.field === 'description') cmp = a.description.localeCompare(b.description)
        else if (sort.field === 'category') cmp = a.category.localeCompare(b.category)
        return sort.dir === 'desc' ? -cmp : cmp
      })
    : []

  return (
    <section className="card animate-fade-in">
      {/* Header */}
      <div className="px-6 pt-6 pb-4">
        <div className="flex items-center justify-between mb-4">
          <div>
            <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-0.5">
              Transaction History
            </p>
            <h2 className="section-title text-lg">The Sacred Timeline</h2>
          </div>
          {data && (
            <div className="flex items-center gap-2">
              {isFetching && (
                <div className="w-3 h-3 rounded-full border border-gold/40 border-t-gold animate-spin" />
              )}
              <span className="text-xs font-mono text-text-muted">
                {data.totalCount.toLocaleString()} records
              </span>
            </div>
          )}
        </div>

        <TransactionFilters />
      </div>

      <div className="divider-gold" />

      {/* Table */}
      {isError && <ErrorState />}

      {isLoading ? (
        <TransactionTableSkeleton />
      ) : (
        <>
          {/* Desktop table */}
          <div className="overflow-x-auto">
            <table className="w-full min-w-[640px]">
              <thead>
                <tr className="border-b border-border">
                  <SortTh
                    field="date"
                    current={sort}
                    onClick={handleSort}
                    className="w-[110px]"
                  >
                    Date
                  </SortTh>
                  <SortTh
                    field="description"
                    current={sort}
                    onClick={handleSort}
                  >
                    Description
                  </SortTh>
                  <SortTh
                    field="category"
                    current={sort}
                    onClick={handleSort}
                    className="w-[130px] hidden sm:table-cell"
                  >
                    Category
                  </SortTh>
                  <th className="px-4 py-3 text-left text-xs font-mono text-text-muted uppercase tracking-wider hidden md:table-cell">
                    Realm
                  </th>
                  <SortTh
                    field="amount"
                    current={sort}
                    onClick={handleSort}
                    className="w-[120px] text-right"
                  >
                    Amount
                  </SortTh>
                </tr>
              </thead>
              <tbody>
                {sortedItems.length === 0 && !isLoading && (
                  <tr>
                    <td colSpan={5}>
                      <EmptyState message="Nothing to see here, mortal." />
                    </td>
                  </tr>
                )}
                {sortedItems.map((tx, i) => (
                  <TransactionRow key={tx.id} tx={tx} index={i} />
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between px-6 py-4 border-t border-border">
              <span className="text-xs font-mono text-text-muted">
                Page {data.page} of {data.totalPages}
              </span>
              <div className="flex items-center gap-1">
                <Button
                  variant="ghost"
                  size="icon"
                  disabled={!data.hasPreviousPage}
                  onClick={() => setFilter({ page: (filter.page ?? 1) - 1 })}
                >
                  <ChevronLeft size={14} />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  disabled={!data.hasNextPage}
                  onClick={() => setFilter({ page: (filter.page ?? 1) + 1 })}
                >
                  <ChevronRight size={14} />
                </Button>
              </div>
            </div>
          )}
        </>
      )}
    </section>
  )
}

function TransactionRow({ tx, index }: { tx: TransactionDto; index: number }) {
  const isCredit = tx.transactionType === 'Credit'

  return (
    <tr
      className={cn(
        'table-row-hover border-b border-border/50 last:border-0',
        'animate-fade-in',
      )}
      style={{ animationDelay: `${Math.min(index * 20, 200)}ms` }}
    >
      {/* Date */}
      <td className="px-4 py-3 text-xs font-mono text-text-muted whitespace-nowrap">
        {formatDateShort(tx.date)}
      </td>

      {/* Description */}
      <td className="px-4 py-3">
        <div className="flex items-center gap-2">
          <span className="text-sm text-text-primary font-medium leading-snug line-clamp-1">
            {tx.description}
          </span>
        </div>
      </td>

      {/* Category */}
      <td className="px-4 py-3 hidden sm:table-cell">
        <span
          className={cn(
            'text-xs font-mono border rounded px-1.5 py-0.5 whitespace-nowrap',
            getCategoryColor(tx.category),
          )}
        >
          {tx.category}
        </span>
      </td>

      {/* Realm */}
      <td className="px-4 py-3 hidden md:table-cell">
        <span className="text-xs font-mono text-text-muted truncate max-w-[120px] block">
          {tx.accountName}
        </span>
      </td>

      {/* Amount */}
      <td className="px-4 py-3 text-right">
        <span
          className={cn(
            'font-mono font-semibold text-sm tabular-nums whitespace-nowrap',
            isCredit ? 'text-worthy' : 'text-text-primary',
          )}
        >
          {isCredit ? '+' : '-'}{formatUsd(tx.amount)}
        </span>
        <div className="flex justify-end mt-0.5">
          <Badge variant={isCredit ? 'credit' : 'debit'} className="text-[10px] px-1 py-0">
            {tx.transactionType}
          </Badge>
        </div>
      </td>
    </tr>
  )
}

function SortTh({
  field,
  current,
  onClick,
  children,
  className,
}: {
  field: SortField
  current: { field: SortField; dir: SortDir }
  onClick: (f: SortField) => void
  children: React.ReactNode
  className?: string
}) {
  const active = current.field === field
  return (
    <th
      className={cn(
        'px-4 py-3 text-left text-xs font-mono text-text-muted uppercase tracking-wider',
        'cursor-pointer select-none hover:text-text-primary transition-colors',
        active && 'text-gold',
        className,
      )}
      onClick={() => onClick(field)}
    >
      <div className="flex items-center gap-1">
        {children}
        {active ? (
          current.dir === 'desc' ? (
            <ChevronDown size={11} />
          ) : (
            <ChevronUp size={11} />
          )
        ) : (
          <span className="opacity-20">
            <ChevronDown size={11} />
          </span>
        )}
      </div>
    </th>
  )
}

function TransactionTableSkeleton() {
  return (
    <div className="p-4 space-y-2">
      {[...Array(8)].map((_, i) => (
        <div key={i} className="flex items-center gap-4 py-2">
          <Skeleton className="h-4 w-20 shrink-0" />
          <Skeleton className="h-4 flex-1" />
          <Skeleton className="h-4 w-24 hidden sm:block shrink-0" />
          <Skeleton className="h-4 w-20 hidden md:block shrink-0" />
          <Skeleton className="h-4 w-24 shrink-0" />
        </div>
      ))}
    </div>
  )
}
