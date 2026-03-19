import { AppHeader } from '@/components/dashboard/AppHeader'
import { HeimdallChat } from '@/components/chat/HeimdallChat'
import { BifrostImport } from '@/components/import/BifrostImport'
import { Dashboard } from '@/pages/Dashboard'

export function App() {
  return (
    <div className="min-h-screen bg-bg-primary text-text-primary">
      <AppHeader />
      <Dashboard />
      <HeimdallChat />
      <BifrostImport />
    </div>
  )
}
