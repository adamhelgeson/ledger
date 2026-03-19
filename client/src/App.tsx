import { AppHeader } from '@/components/dashboard/AppHeader'
import { HeimdallChat } from '@/components/chat/HeimdallChat'
import { BifrostImport } from '@/components/import/BifrostImport'
import { Dashboard } from '@/pages/Dashboard'
import { SettingsPage } from '@/pages/SettingsPage'
import { useDashboardStore } from '@/stores/dashboardStore'

export function App() {
  const currentView = useDashboardStore((s) => s.currentView)

  return (
    <div className="min-h-screen bg-bg-primary text-text-primary">
      <AppHeader />
      {currentView === 'dashboard' ? <Dashboard /> : <SettingsPage />}
      <HeimdallChat />
      <BifrostImport />
    </div>
  )
}
