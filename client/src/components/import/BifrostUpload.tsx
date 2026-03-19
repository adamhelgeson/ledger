import { useRef, useState } from 'react'
import { CloudUpload } from 'lucide-react'
import type { AccountDto } from '@/types/api'
import { cn } from '@/lib/utils'

interface Props {
  accounts: AccountDto[]
  onUpload: (accountId: string, file: File) => void
  loading: boolean
}

export function BifrostUpload({ accounts, onUpload, loading }: Props) {
  const [accountId, setAccountId] = useState(accounts[0]?.id ?? '')
  const [isDragging, setIsDragging] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  const handleFile = (file: File | undefined) => {
    if (!file || !accountId) return
    if (!file.name.endsWith('.csv')) return
    onUpload(accountId, file)
  }

  return (
    <div className="space-y-6">
      {/* Account selector */}
      <div>
        <label className="block text-xs font-mono text-text-muted uppercase tracking-widest mb-2">
          Destination Realm
        </label>
        <select
          value={accountId}
          onChange={(e) => setAccountId(e.target.value)}
          disabled={loading}
          className={cn(
            'w-full bg-bg-primary border border-border rounded-lg px-3 py-2',
            'text-sm text-text-primary font-mono',
            'focus:outline-none focus:ring-1 focus:ring-gold/40',
            'disabled:opacity-50',
          )}
        >
          {accounts.map((a) => (
            <option key={a.id} value={a.id}>
              {a.name} — {a.institution}
            </option>
          ))}
        </select>
      </div>

      {/* Drop zone */}
      <div
        role="button"
        tabIndex={0}
        aria-label="Drop CSV file or click to upload"
        onClick={() => !loading && inputRef.current?.click()}
        onKeyDown={(e) => e.key === 'Enter' && !loading && inputRef.current?.click()}
        onDragOver={(e) => {
          e.preventDefault()
          setIsDragging(true)
        }}
        onDragLeave={() => setIsDragging(false)}
        onDrop={(e) => {
          e.preventDefault()
          setIsDragging(false)
          handleFile(e.dataTransfer.files[0])
        }}
        className={cn(
          'relative overflow-hidden rounded-xl border-2 border-dashed',
          'flex flex-col items-center justify-center gap-4 py-14 px-6',
          'cursor-pointer transition-all duration-300',
          isDragging
            ? [
                'border-transparent',
                // Rainbow border via pseudo-element is hard in Tailwind; use outline trick
                'ring-2 ring-offset-2 ring-offset-bg-card ring-purple-500',
                'bg-bifrost-gradient bg-[length:400%_400%] animate-bifrost opacity-90',
              ]
            : 'border-border hover:border-gold/30 bg-bg-primary hover:bg-bg-elevated',
          loading && 'pointer-events-none opacity-60',
        )}
      >
        {/* Bifrost shimmer overlay when dragging */}
        {isDragging && (
          <div className="absolute inset-0 bg-bifrost-gradient bg-[length:400%_400%] animate-bifrost opacity-20" />
        )}

        <div
          className={cn(
            'relative z-10 flex flex-col items-center gap-3 text-center',
            isDragging && 'text-white',
          )}
        >
          <div
            className={cn(
              'w-14 h-14 rounded-full flex items-center justify-center transition-colors',
              isDragging ? 'bg-white/20' : 'bg-gold/10 border border-gold/20',
            )}
          >
            <CloudUpload size={24} className={isDragging ? 'text-white' : 'text-gold'} />
          </div>

          <div>
            <p className={cn('font-display font-bold text-sm', isDragging ? 'text-white' : 'text-gold')}>
              {isDragging ? 'Release to summon the Bifrost' : 'Drop your CSV here'}
            </p>
            <p className={cn('text-xs mt-1', isDragging ? 'text-white/70' : 'text-text-muted')}>
              or click to browse — Chase and generic CSV formats supported
            </p>
          </div>
        </div>

        <input
          ref={inputRef}
          type="file"
          accept=".csv"
          className="hidden"
          onChange={(e) => handleFile(e.target.files?.[0])}
          disabled={loading}
        />
      </div>
    </div>
  )
}
