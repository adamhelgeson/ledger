import { useCallback, useState } from 'react'
import { Search, X, SlidersHorizontal } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Select } from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import { cn, debounce } from '@/lib/utils'
import { useAccounts } from '@/hooks/useAccounts'
import { useDashboardStore } from '@/stores/dashboardStore'
import { formatDateIso } from '@/lib/formatters'

const CATEGORIES = [
  'Groceries', 'Dining', 'Subscriptions', 'Gas', 'Utilities',
  'Shopping', 'Health', 'Transport', 'Income', 'Housing', 'Transfers', 'Other',
]

export function TransactionFilters() {
  const { filter, setFilter, resetFilter } = useDashboardStore()
  const { data: accounts } = useAccounts()
  const [searchInput, setSearchInput] = useState(filter.search ?? '')
  const [expanded, setExpanded] = useState(false)

  const hasActiveFilters =
    !!filter.accountId || !!filter.category || !!filter.from || !!filter.to || !!filter.search

  const debouncedSearch = useCallback(
    debounce((value: string) => setFilter({ search: value || undefined }), 350),
    [setFilter],
  )

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchInput(e.target.value)
    debouncedSearch(e.target.value)
  }

  const handleClear = () => {
    setSearchInput('')
    resetFilter()
  }

  // Quick date range helpers
  const setDateRange = (months: number) => {
    const to = new Date()
    const from = new Date()
    from.setMonth(from.getMonth() - months)
    setFilter({ from: formatDateIso(from), to: formatDateIso(to) })
  }

  return (
    <div className="space-y-3">
      {/* Search + expand toggle row */}
      <div className="flex items-center gap-2">
        <div className="relative flex-1">
          <Search
            size={14}
            className="absolute left-3 top-1/2 -translate-y-1/2 text-text-muted pointer-events-none"
          />
          <Input
            className="pl-8 pr-8 font-mono text-sm"
            placeholder="Search the Sacred Timeline..."
            value={searchInput}
            onChange={handleSearchChange}
          />
          {searchInput && (
            <button
              onClick={() => {
                setSearchInput('')
                setFilter({ search: undefined })
              }}
              className="absolute right-2.5 top-1/2 -translate-y-1/2 text-text-muted hover:text-text-primary transition-colors"
            >
              <X size={12} />
            </button>
          )}
        </div>

        <Button
          variant="secondary"
          size="icon"
          onClick={() => setExpanded((e) => !e)}
          className={cn(hasActiveFilters && 'border-gold/40 text-gold')}
          title="More filters"
        >
          <SlidersHorizontal size={14} />
        </Button>

        {hasActiveFilters && (
          <Button variant="ghost" size="sm" onClick={handleClear} className="text-text-muted">
            Clear
          </Button>
        )}
      </div>

      {/* Expanded filters */}
      {expanded && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 animate-fade-in">
          {/* Realm filter */}
          <Select
            value={filter.accountId ?? ''}
            onChange={(e) => setFilter({ accountId: e.target.value || undefined })}
            aria-label="Filter by realm"
          >
            <option value="">All Realms</option>
            {accounts?.map((a) => (
              <option key={a.id} value={a.id}>
                {a.name}
              </option>
            ))}
          </Select>

          {/* Category filter */}
          <Select
            value={filter.category ?? ''}
            onChange={(e) => setFilter({ category: e.target.value || undefined })}
            aria-label="Filter by category"
          >
            <option value="">All Categories</option>
            {CATEGORIES.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </Select>

          {/* Date from */}
          <input
            type="date"
            value={filter.from ?? ''}
            onChange={(e) => setFilter({ from: e.target.value || undefined })}
            className="h-9 rounded-lg border border-border bg-bg-elevated px-3 text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-gold/40 focus:border-gold/50 transition-all duration-150 cursor-pointer"
            aria-label="From date"
          />
          <input
            type="date"
            value={filter.to ?? ''}
            onChange={(e) => setFilter({ to: e.target.value || undefined })}
            className="h-9 rounded-lg border border-border bg-bg-elevated px-3 text-sm text-text-primary focus:outline-none focus:ring-2 focus:ring-gold/40 focus:border-gold/50 transition-all duration-150 cursor-pointer"
            aria-label="To date"
          />
        </div>
      )}

      {/* Quick range pills */}
      {expanded && (
        <div className="flex items-center gap-2">
          <span className="text-xs text-text-muted font-mono">Quick:</span>
          {[
            { label: '30d', months: 1 },
            { label: '3 months', months: 3 },
            { label: '6 months', months: 6 },
          ].map((r) => (
            <button
              key={r.label}
              onClick={() => setDateRange(r.months)}
              className="filter-pill text-xs"
            >
              {r.label}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
