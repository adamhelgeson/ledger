import { AlertTriangle } from 'lucide-react'
import type { ImportPreviewDto } from '@/types/api'
import { formatUsd, formatDateShort } from '@/lib/formatters'
import { getCategoryColor } from '@/lib/utils'
import { cn } from '@/lib/utils'

interface Props {
  preview: ImportPreviewDto
  selected: Set<number>
  onToggle: (index: number) => void
  onSelectAll: () => void
  onDeselectAll: () => void
}

export function ImportPreviewTable({
  preview,
  selected,
  onToggle,
  onSelectAll,
  onDeselectAll,
}: Props) {
  const allSelected = selected.size === preview.rows.length
  const someSelected = selected.size > 0 && !allSelected

  return (
    <div className="space-y-4">
      {/* Parse errors */}
      {preview.errors.length > 0 && (
        <div className="rounded-lg border border-unworthy/30 bg-unworthy/5 p-3">
          <div className="flex items-center gap-2 mb-2">
            <AlertTriangle size={13} className="text-unworthy shrink-0" />
            <span className="text-xs font-mono font-bold text-unworthy uppercase tracking-widest">
              Parse Warnings ({preview.errors.length})
            </span>
          </div>
          <ul className="space-y-0.5">
            {preview.errors.map((err, i) => (
              <li key={i} className="text-xs font-mono text-text-secondary">
                {err}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Summary bar */}
      <div className="flex items-center justify-between">
        <p className="text-xs font-mono text-text-muted">
          <span className="text-text-primary font-bold">{selected.size}</span> of{' '}
          {preview.rows.length} rows selected
          {preview.detectedParser && (
            <span className="ml-2 text-gold/60">· {preview.detectedParser} format</span>
          )}
        </p>
        <button
          onClick={allSelected ? onDeselectAll : onSelectAll}
          className="text-xs font-mono text-blue-accent hover:text-blue-accent/80 transition-colors"
        >
          {allSelected ? 'Deselect all' : 'Select all'}
        </button>
      </div>

      {/* Table */}
      <div className="overflow-x-auto rounded-lg border border-border">
        <table className="w-full text-xs">
          <thead>
            <tr className="border-b border-border bg-bg-elevated">
              <th className="w-8 px-3 py-2 text-left">
                <input
                  type="checkbox"
                  checked={allSelected}
                  ref={(el) => el && (el.indeterminate = someSelected)}
                  onChange={allSelected ? onDeselectAll : onSelectAll}
                  className="accent-gold"
                />
              </th>
              <th className="px-3 py-2 text-left font-mono text-text-muted uppercase tracking-widest">
                Date
              </th>
              <th className="px-3 py-2 text-left font-mono text-text-muted uppercase tracking-widest">
                Description
              </th>
              <th className="px-3 py-2 text-left font-mono text-text-muted uppercase tracking-widest">
                Category
              </th>
              <th className="px-3 py-2 text-right font-mono text-text-muted uppercase tracking-widest">
                Amount
              </th>
              <th className="px-3 py-2 text-left font-mono text-text-muted uppercase tracking-widest">
                Type
              </th>
            </tr>
          </thead>
          <tbody>
            {preview.rows.map((row, i) => {
              const checked = selected.has(i)
              return (
                <tr
                  key={i}
                  onClick={() => onToggle(i)}
                  className={cn(
                    'border-b border-border/50 cursor-pointer transition-colors',
                    checked
                      ? 'bg-gold/5 hover:bg-gold/8'
                      : 'hover:bg-bg-elevated opacity-50',
                  )}
                >
                  <td className="px-3 py-2">
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => onToggle(i)}
                      onClick={(e) => e.stopPropagation()}
                      className="accent-gold"
                    />
                  </td>
                  <td className="px-3 py-2 font-mono text-text-secondary whitespace-nowrap">
                    {formatDateShort(row.date)}
                  </td>
                  <td className="px-3 py-2 text-text-primary max-w-[200px] truncate">
                    {row.description}
                  </td>
                  <td className="px-3 py-2">
                    <span
                      className="px-1.5 py-0.5 rounded text-[10px] font-mono"
                      style={{ backgroundColor: `${getCategoryColor(row.category)}20`, color: getCategoryColor(row.category) }}
                    >
                      {row.category}
                    </span>
                  </td>
                  <td className="px-3 py-2 text-right font-mono whitespace-nowrap">
                    <span className={row.transactionType === 'Credit' ? 'text-worthy' : 'text-text-primary'}>
                      {row.transactionType === 'Credit' ? '+' : '-'}
                      {formatUsd(row.amount)}
                    </span>
                  </td>
                  <td className="px-3 py-2">
                    <span className={cn(
                      'text-[10px] font-mono uppercase',
                      row.transactionType === 'Credit' ? 'text-worthy' : 'text-text-muted',
                    )}>
                      {row.transactionType}
                    </span>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </div>
  )
}
