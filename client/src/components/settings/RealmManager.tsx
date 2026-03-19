import { useState } from 'react'
import { Plus, Pencil, Archive, Trash2, Globe } from 'lucide-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { accountsApi } from '@/services/api'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { formatUsd } from '@/lib/formatters'
import type { AccountDto } from '@/types/api'
import { AccountFormModal } from './AccountFormModal'

const ACCOUNT_TYPE_LABELS: Record<string, string> = {
  Checking: 'Checking',
  Savings: 'Savings',
  CreditCard: 'Credit Card',
  Brokerage: 'Brokerage',
  Retirement401k: '401(k)',
}

export function RealmManager() {
  const queryClient = useQueryClient()
  const { data: accounts, isLoading } = useQuery({
    queryKey: ['accounts'],
    queryFn: () => accountsApi.getAll(),
    staleTime: 30_000,
  })

  const [modalState, setModalState] = useState<
    { mode: 'create' } | { mode: 'edit'; account: AccountDto } | null
  >(null)

  const deleteMutation = useMutation({
    mutationFn: (id: string) => accountsApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['accounts'] }),
  })

  const archiveMutation = useMutation({
    mutationFn: (account: AccountDto) =>
      accountsApi.update(account.id, { ...account, isActive: !account.isActive }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['accounts'] }),
  })

  const handleDelete = (account: AccountDto) => {
    if (window.confirm(`Delete "${account.name}"? This cannot be undone.`)) {
      deleteMutation.mutate(account.id)
    }
  }

  const active = accounts?.filter((a) => a.isActive) ?? []
  const archived = accounts?.filter((a) => !a.isActive) ?? []

  return (
    <>
      <section className="card">
        <div className="px-6 pt-6 pb-4 flex items-center justify-between">
          <div>
            <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-0.5">
              Accounts
            </p>
            <h2 className="section-title text-base">The Nine Realms</h2>
          </div>
          <Button
            size="sm"
            className="gap-1.5"
            onClick={() => setModalState({ mode: 'create' })}
          >
            <Plus size={13} />
            <span className="font-mono text-xs">Add Realm</span>
          </Button>
        </div>

        <div className="divider-gold" />

        {isLoading ? (
          <div className="p-6 space-y-3">
            {[...Array(4)].map((_, i) => (
              <Skeleton key={i} className="h-14 w-full" />
            ))}
          </div>
        ) : (
          <div className="divide-y divide-border/50">
            {active.length === 0 && archived.length === 0 && (
              <p className="px-6 py-8 text-sm text-text-muted text-center font-mono">
                Nothing to see here, mortal. Add your first realm.
              </p>
            )}

            {active.map((account) => (
              <AccountRow
                key={account.id}
                account={account}
                onEdit={() => setModalState({ mode: 'edit', account })}
                onArchive={() => archiveMutation.mutate(account)}
                onDelete={() => handleDelete(account)}
              />
            ))}

            {archived.length > 0 && (
              <>
                <div className="px-6 py-2 bg-bg-primary/40">
                  <span className="text-[10px] font-mono text-text-muted uppercase tracking-widest">
                    Archived Realms
                  </span>
                </div>
                {archived.map((account) => (
                  <AccountRow
                    key={account.id}
                    account={account}
                    onEdit={() => setModalState({ mode: 'edit', account })}
                    onArchive={() => archiveMutation.mutate(account)}
                    onDelete={() => handleDelete(account)}
                  />
                ))}
              </>
            )}
          </div>
        )}
      </section>

      {modalState && (
        <AccountFormModal
          mode={modalState.mode}
          account={modalState.mode === 'edit' ? modalState.account : undefined}
          onClose={() => setModalState(null)}
        />
      )}
    </>
  )
}

function AccountRow({
  account,
  onEdit,
  onArchive,
  onDelete,
}: {
  account: AccountDto
  onEdit: () => void
  onArchive: () => void
  onDelete: () => void
}) {
  return (
    <div
      className={cn(
        'flex items-center gap-4 px-6 py-4 group',
        !account.isActive && 'opacity-50',
      )}
    >
      {/* Icon */}
      <div className="w-8 h-8 rounded-full bg-gold/10 border border-gold/20 flex items-center justify-center shrink-0">
        <Globe size={14} className="text-gold" />
      </div>

      {/* Info */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className="text-sm font-medium text-text-primary truncate">{account.name}</span>
          {!account.isActive && (
            <span className="text-[10px] font-mono px-1.5 py-0.5 rounded border border-border text-text-muted">
              archived
            </span>
          )}
        </div>
        <div className="flex items-center gap-2 mt-0.5">
          <span className="text-xs font-mono text-text-muted">{account.institution}</span>
          <span className="text-text-muted/30">·</span>
          <span className="text-xs font-mono text-text-muted">
            {ACCOUNT_TYPE_LABELS[account.accountType] ?? account.accountType}
          </span>
        </div>
      </div>

      {/* Balance */}
      <div className="text-right hidden sm:block">
        <span className="text-sm font-mono font-semibold tabular-nums">
          {account.currentBalance != null ? formatUsd(Math.abs(account.currentBalance)) : '—'}
        </span>
        <div className="text-[10px] font-mono text-text-muted">{account.currency}</div>
      </div>

      {/* Actions — visible on hover */}
      <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
        <Button variant="ghost" size="icon" onClick={onEdit} title="Edit">
          <Pencil size={13} />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          onClick={onArchive}
          title={account.isActive ? 'Archive' : 'Restore'}
        >
          <Archive size={13} className={account.isActive ? '' : 'text-gold'} />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          onClick={onDelete}
          title="Delete"
          className="hover:text-red-400"
        >
          <Trash2 size={13} />
        </Button>
      </div>
    </div>
  )
}
