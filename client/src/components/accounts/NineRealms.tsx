import { useAccounts } from '@/hooks/useAccounts'
import { AccountCard } from './AccountCard'
import { CardSkeleton } from '@/components/ui/LoadingState'
import { ErrorState, EmptyState } from '@/components/ui/LoadingState'

export function NineRealms() {
  const { data: accounts, isLoading, isError } = useAccounts()

  return (
    <section>
      {/* Section header */}
      <div className="flex items-center justify-between mb-4">
        <div>
          <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-0.5">
            Account Overview
          </p>
          <h2 className="section-title text-lg">The Nine Realms</h2>
        </div>
        {accounts && (
          <span className="text-xs font-mono text-text-muted">
            {accounts.filter((a) => a.isActive).length} active realms
          </span>
        )}
      </div>

      <div className="divider-gold mb-4" />

      {isLoading && (
        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
          {[0, 1, 2, 3, 4].map((i) => (
            <CardSkeleton key={i} />
          ))}
        </div>
      )}

      {isError && <ErrorState />}

      {accounts && accounts.length === 0 && (
        <EmptyState message="No realms found. Even Thor had slow months." />
      )}

      {accounts && accounts.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
          {accounts.map((account) => (
            <AccountCard key={account.id} account={account} />
          ))}
        </div>
      )}
    </section>
  )
}
