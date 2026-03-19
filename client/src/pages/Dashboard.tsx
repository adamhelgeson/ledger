import { VaultOfAsgard } from '@/components/dashboard/VaultOfAsgard'
import { RagnarokReport } from '@/components/dashboard/RagnarokReport'
import { NineRealms } from '@/components/accounts/NineRealms'
import { SacredTimeline } from '@/components/transactions/SacredTimeline'

export function Dashboard() {
  return (
    <main className="max-w-screen-2xl mx-auto px-4 sm:px-6 py-8 space-y-10">
      {/* Net worth hero */}
      <VaultOfAsgard />

      {/* Accounts + spending chart — side by side on xl */}
      <div className="grid grid-cols-1 xl:grid-cols-5 gap-8">
        <div className="xl:col-span-3">
          <NineRealms />
        </div>
        <div className="xl:col-span-2">
          <RagnarokReport />
        </div>
      </div>

      {/* Transactions */}
      <SacredTimeline />
    </main>
  )
}
