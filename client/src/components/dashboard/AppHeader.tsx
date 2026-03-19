import { MessageSquare, RefreshCw, Zap } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useDashboardStore } from '@/stores/dashboardStore'
import { useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { cn } from '@/lib/utils'

export function AppHeader() {
  const setChatOpen = useDashboardStore((s) => s.setChatOpen)
  const chatOpen = useDashboardStore((s) => s.chatOpen)
  const setBifrostOpen = useDashboardStore((s) => s.setBifrostOpen)
  const queryClient = useQueryClient()
  const [refreshing, setRefreshing] = useState(false)

  const handleRefresh = async () => {
    setRefreshing(true)
    await queryClient.invalidateQueries()
    setTimeout(() => setRefreshing(false), 600)
  }

  return (
    <header className="sticky top-0 z-30 border-b border-border bg-bg-primary/90 backdrop-blur-sm">
      <div className="max-w-screen-2xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between">
        {/* Logo */}
        <div className="flex items-center gap-3">
          <div className="w-7 h-7 rounded bg-gold/10 border border-gold/20 flex items-center justify-center">
            <span className="text-gold text-xs font-bold font-display">L</span>
          </div>
          <div className="leading-none">
            <h1 className="font-display text-base font-bold text-gold tracking-wide">Ledger</h1>
            <p className="text-[10px] font-mono text-text-muted tracking-widest uppercase leading-none mt-0.5">
              The All-Father's Treasury
            </p>
          </div>
        </div>

        {/* Gold divider accent */}
        <div className="hidden sm:block h-5 w-px bg-gold/20 mx-2" />

        {/* Nav links — placeholder for future pages */}
        <nav className="hidden sm:flex items-center gap-1 flex-1 ml-2">
          <button className="filter-pill active text-xs">Dashboard</button>
        </nav>

        {/* Actions */}
        <div className="flex items-center gap-2">
          <Button
            variant="secondary"
            size="sm"
            onClick={() => setBifrostOpen(true)}
            className="gap-1.5"
            title="Import CSV — The Bifrost"
          >
            <Zap size={13} className="text-gold" />
            <span className="hidden sm:inline font-mono text-xs">Bifrost</span>
          </Button>
          <Button
            variant="ghost"
            size="icon"
            onClick={handleRefresh}
            title="Refresh data"
          >
            <RefreshCw size={14} className={cn(refreshing && 'animate-spin text-gold')} />
          </Button>

          <Button
            variant={chatOpen ? 'default' : 'secondary'}
            size="sm"
            onClick={() => setChatOpen(!chatOpen)}
            className="gap-1.5"
          >
            <MessageSquare size={13} />
            <span className="hidden sm:inline font-mono text-xs">Heimdall</span>
          </Button>
        </div>
      </div>
    </header>
  )
}
