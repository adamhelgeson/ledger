import { RealmManager } from '@/components/settings/RealmManager'

export function SettingsPage() {
  return (
    <main className="max-w-screen-2xl mx-auto px-4 sm:px-6 py-8 space-y-8">
      {/* Page header */}
      <div className="border-b border-border pb-6">
        <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-0.5">
          Configuration
        </p>
        <h1 className="font-display text-2xl font-bold text-gold">The Nine Realms — Settings</h1>
        <p className="text-sm text-text-muted mt-1">
          Manage your accounts, realms, and treasury configuration.
        </p>
      </div>

      {/* Realm Manager */}
      <RealmManager />

      {/* About section */}
      <section className="card px-6 py-6">
        <p className="text-xs font-mono text-text-muted uppercase tracking-widest mb-1">About</p>
        <h2 className="section-title text-base mb-3">Ledger — The All-Father's Treasury</h2>
        <p className="text-sm text-text-secondary leading-relaxed">
          Forged in the fires of Nidavellir. Powered by C#, React, and an unhealthy obsession
          with budgeting.
        </p>
        <div className="mt-4 flex flex-wrap gap-2">
          {['ASP.NET Core 8', 'React 18', 'TypeScript', 'SQLite', 'TanStack Query', 'Tailwind CSS', 'Claude AI'].map(
            (tech) => (
              <span
                key={tech}
                className="text-[11px] font-mono px-2 py-0.5 rounded border border-border text-text-muted bg-bg-primary"
              >
                {tech}
              </span>
            ),
          )}
        </div>
      </section>
    </main>
  )
}
