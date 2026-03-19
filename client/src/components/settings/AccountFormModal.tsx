import { useState } from 'react'
import { X } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { accountsApi } from '@/services/api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import type { AccountDto, AccountType } from '@/types/api'

const ACCOUNT_TYPES: AccountType[] = [
  'Checking',
  'Savings',
  'CreditCard',
  'Brokerage',
  'Retirement401k',
]

const ACCOUNT_TYPE_LABELS: Record<AccountType, string> = {
  Checking: 'Checking',
  Savings: 'Savings',
  CreditCard: 'Credit Card',
  Brokerage: 'Brokerage',
  Retirement401k: '401(k)',
}

interface Props {
  mode: 'create' | 'edit'
  account?: AccountDto
  onClose: () => void
}

export function AccountFormModal({ mode, account, onClose }: Props) {
  const queryClient = useQueryClient()
  const [name, setName] = useState(account?.name ?? '')
  const [institution, setInstitution] = useState(account?.institution ?? '')
  const [accountType, setAccountType] = useState<AccountType>(account?.accountType ?? 'Checking')
  const [currency, setCurrency] = useState(account?.currency ?? 'USD')
  const [error, setError] = useState<string | null>(null)

  const mutation = useMutation({
    mutationFn: () => {
      const body = { name, institution, accountType, currency }
      return mode === 'create'
        ? accountsApi.create(body)
        : accountsApi.update(account!.id, { ...body, isActive: account!.isActive })
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      onClose()
    },
    onError: (err: Error) => setError(err.message),
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!name.trim()) { setError('Name is required'); return }
    if (!institution.trim()) { setError('Institution is required'); return }
    setError(null)
    mutation.mutate()
  }

  return (
    <>
      {/* Backdrop */}
      <div
        className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Modal */}
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div className="bg-bg-card border border-border rounded-xl shadow-2xl w-full max-w-md animate-fade-in">
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-border">
            <div>
              <h2 className="font-display text-sm font-bold text-gold tracking-wide">
                {mode === 'create' ? 'Add New Realm' : 'Edit Realm'}
              </h2>
              <p className="text-[10px] font-mono text-text-muted uppercase tracking-widest">
                {mode === 'create' ? 'Expand your treasury' : 'Update realm details'}
              </p>
            </div>
            <Button variant="ghost" size="icon" onClick={onClose}>
              <X size={16} />
            </Button>
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
            {error && (
              <p className="text-xs font-mono text-red-400 bg-red-400/10 border border-red-400/20 rounded px-3 py-2">
                {error}
              </p>
            )}

            <div className="space-y-1.5">
              <label className="text-xs font-mono text-text-muted uppercase tracking-wider">
                Realm Name
              </label>
              <Input
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. Asgard Checking"
                className="font-mono text-sm"
                autoFocus
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-mono text-text-muted uppercase tracking-wider">
                Institution
              </label>
              <Input
                value={institution}
                onChange={(e) => setInstitution(e.target.value)}
                placeholder="e.g. Chase, Fidelity"
                className="font-mono text-sm"
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-mono text-text-muted uppercase tracking-wider">
                Account Type
              </label>
              <select
                value={accountType}
                onChange={(e) => setAccountType(e.target.value as AccountType)}
                className="w-full bg-bg-primary border border-border rounded-md px-3 py-2 text-sm font-mono text-text-primary focus:outline-none focus:ring-1 focus:ring-gold/50"
              >
                {ACCOUNT_TYPES.map((t) => (
                  <option key={t} value={t}>
                    {ACCOUNT_TYPE_LABELS[t]}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-mono text-text-muted uppercase tracking-wider">
                Currency
              </label>
              <Input
                value={currency}
                onChange={(e) => setCurrency(e.target.value.toUpperCase())}
                placeholder="USD"
                maxLength={3}
                className="font-mono text-sm w-24"
              />
            </div>

            <div className="flex gap-2 pt-2">
              <Button
                type="submit"
                className="flex-1"
                disabled={mutation.isPending}
              >
                {mutation.isPending
                  ? 'Forging...'
                  : mode === 'create'
                  ? 'Add Realm'
                  : 'Save Changes'}
              </Button>
              <Button type="button" variant="secondary" onClick={onClose}>
                Cancel
              </Button>
            </div>
          </form>
        </div>
      </div>
    </>
  )
}
