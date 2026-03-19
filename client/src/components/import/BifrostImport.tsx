import { useState } from 'react'
import { X, Zap, ChevronLeft, Loader2 } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'
import { useDashboardStore } from '@/stores/dashboardStore'
import { useAccounts } from '@/hooks/useAccounts'
import { previewImport, confirmImport } from '@/services/api'
import type { ConfirmImportDto, ImportPreviewDto } from '@/types/api'
import { Button } from '@/components/ui/button'
import { BifrostUpload } from './BifrostUpload'
import { ImportPreviewTable } from './ImportPreviewTable'
import { cn } from '@/lib/utils'

type Stage = 'upload' | 'uploading' | 'preview' | 'confirming' | 'success' | 'error'

export function BifrostImport() {
  const bifrostOpen = useDashboardStore((s) => s.bifrostOpen)
  const setBifrostOpen = useDashboardStore((s) => s.setBifrostOpen)
  const queryClient = useQueryClient()

  const { data: accounts = [], isLoading: accountsLoading } = useAccounts()

  const [stage, setStage] = useState<Stage>('upload')
  const [accountId, setAccountId] = useState<string>('')
  const [filename, setFilename] = useState<string>('')
  const [preview, setPreview] = useState<ImportPreviewDto | null>(null)
  const [selected, setSelected] = useState<Set<number>>(new Set())
  const [result, setResult] = useState<ConfirmImportDto | null>(null)
  const [error, setError] = useState<string>('')

  const handleClose = () => {
    setBifrostOpen(false)
    // Reset after transition
    setTimeout(() => {
      setStage('upload')
      setPreview(null)
      setSelected(new Set())
      setResult(null)
      setError('')
    }, 300)
  }

  const handleUpload = async (acctId: string, file: File) => {
    setAccountId(acctId)
    setFilename(file.name)
    setStage('uploading')
    setError('')

    try {
      const data = await previewImport(acctId, file)
      setPreview(data)
      // Pre-select all rows
      setSelected(new Set(data.rows.map((_, i) => i)))
      setStage('preview')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong in the multiverse.')
      setStage('error')
    }
  }

  const handleConfirm = async () => {
    if (!preview || selected.size === 0) return
    setStage('confirming')

    const selectedRows = preview.rows.filter((_, i) => selected.has(i))

    try {
      const data = await confirmImport(accountId, filename, selectedRows)
      setResult(data)
      setStage('success')
      // Refresh dashboard and transaction data
      await queryClient.invalidateQueries()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Import failed. Heimdall weeps.')
      setStage('error')
    }
  }

  const accountName = accounts.find((a) => a.id === accountId)?.name ?? 'your realm'

  return (
    <>
      {/* Backdrop */}
      <div
        className={cn(
          'fixed inset-0 z-40 bg-black/60 backdrop-blur-sm transition-opacity duration-300',
          bifrostOpen ? 'opacity-100 pointer-events-auto' : 'opacity-0 pointer-events-none',
        )}
        onClick={handleClose}
      />

      {/* Modal */}
      <div
        className={cn(
          'fixed inset-x-4 top-1/2 -translate-y-1/2 z-50 mx-auto max-w-2xl',
          'bg-bg-card border border-border rounded-2xl shadow-2xl',
          'flex flex-col max-h-[90vh]',
          'transition-all duration-300',
          bifrostOpen ? 'opacity-100 scale-100' : 'opacity-0 scale-95 pointer-events-none',
        )}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-5 border-b border-border shrink-0">
          <div className="flex items-center gap-3">
            {stage === 'preview' && (
              <button
                onClick={() => setStage('upload')}
                className="text-text-muted hover:text-text-primary transition-colors"
              >
                <ChevronLeft size={16} />
              </button>
            )}
            <div>
              <h2 className="font-display font-bold text-gold tracking-wide">The Bifrost</h2>
              <p className="text-[10px] font-mono text-text-muted uppercase tracking-widest mt-0.5">
                {stage === 'upload' && 'Import transactions from your bank'}
                {stage === 'uploading' && 'Summoning the Bifrost...'}
                {stage === 'preview' && `${preview?.rowCount ?? 0} transactions detected — ${preview?.detectedParser ?? ''} format`}
                {stage === 'confirming' && 'Transporting transactions...'}
                {stage === 'success' && 'Import complete'}
                {stage === 'error' && 'The Bifrost faltered'}
              </p>
            </div>
          </div>
          <Button variant="ghost" size="icon" onClick={handleClose}>
            <X size={16} />
          </Button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto px-6 py-6 min-h-0">
          {/* Upload stage */}
          {(stage === 'upload' || stage === 'uploading') && !accountsLoading && (
            <BifrostUpload
              accounts={accounts}
              onUpload={handleUpload}
              loading={stage === 'uploading'}
            />
          )}

          {/* Loading state */}
          {stage === 'uploading' && (
            <div className="flex flex-col items-center gap-3 py-8 animate-fade-in">
              <Loader2 size={28} className="animate-spin text-gold" />
              <p className="font-mono text-sm text-text-muted">Summoning the Bifrost...</p>
            </div>
          )}

          {/* Preview stage */}
          {stage === 'preview' && preview && (
            <ImportPreviewTable
              preview={preview}
              selected={selected}
              onToggle={(i) => {
                setSelected((prev) => {
                  const next = new Set(prev)
                  next.has(i) ? next.delete(i) : next.add(i)
                  return next
                })
              }}
              onSelectAll={() => setSelected(new Set(preview.rows.map((_, i) => i)))}
              onDeselectAll={() => setSelected(new Set())}
            />
          )}

          {/* Confirming */}
          {stage === 'confirming' && (
            <div className="flex flex-col items-center gap-3 py-12 animate-fade-in">
              <Loader2 size={28} className="animate-spin text-gold" />
              <p className="font-mono text-sm text-text-muted">
                Transporting transactions across the Nine Realms...
              </p>
            </div>
          )}

          {/* Success */}
          {stage === 'success' && result && (
            <div className="flex flex-col items-center gap-5 py-10 text-center animate-fade-in">
              <div className="w-16 h-16 rounded-full bg-worthy/10 border border-worthy/30 flex items-center justify-center">
                <Zap size={28} className="text-worthy" />
              </div>
              <div>
                <p className="font-display font-bold text-xl text-gold">
                  {result.importedCount} transactions arrived safely
                </p>
                <p className="font-mono text-sm text-text-muted mt-1">
                  in <span className="text-text-primary">{accountName}</span>
                  {result.skippedCount > 0 && (
                    <span> · {result.skippedCount} duplicates skipped</span>
                  )}
                </p>
              </div>
              <Button onClick={handleClose} className="mt-2">
                Close the Bifrost
              </Button>
            </div>
          )}

          {/* Error */}
          {stage === 'error' && (
            <div className="flex flex-col items-center gap-4 py-10 text-center animate-fade-in">
              <p className="font-display font-bold text-banished">
                Something went wrong in the multiverse.
              </p>
              <p className="font-mono text-sm text-text-muted">{error}</p>
              <Button variant="secondary" onClick={() => setStage('upload')}>
                Try again
              </Button>
            </div>
          )}
        </div>

        {/* Footer — only shown on preview stage */}
        {stage === 'preview' && (
          <div className="px-6 py-4 border-t border-border shrink-0 flex items-center justify-between gap-3">
            <p className="text-xs font-mono text-text-muted">
              {selected.size === 0 && 'Select rows to import'}
              {selected.size > 0 && `${selected.size} row${selected.size !== 1 ? 's' : ''} will be imported`}
            </p>
            <div className="flex gap-2">
              <Button variant="secondary" onClick={handleClose}>
                Cancel
              </Button>
              <Button
                onClick={handleConfirm}
                disabled={selected.size === 0}
                className="gap-1.5"
              >
                <Zap size={13} />
                Import {selected.size > 0 ? selected.size : ''} transactions
              </Button>
            </div>
          </div>
        )}
      </div>
    </>
  )
}
