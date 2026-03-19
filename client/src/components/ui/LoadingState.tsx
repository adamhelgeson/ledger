import { Skeleton } from './skeleton'

export function LoadingState({ message = 'Summoning the Bifrost...' }: { message?: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 gap-4">
      <div className="w-8 h-8 rounded-full border-2 border-gold/20 border-t-gold animate-spin" />
      <p className="text-text-muted text-sm font-mono">{message}</p>
    </div>
  )
}

export function ErrorState({ message = 'Something went wrong in the multiverse.' }: { message?: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-20 gap-3">
      <p className="text-2xl">⚡</p>
      <p className="text-text-secondary text-sm">{message}</p>
    </div>
  )
}

export function EmptyState({ message = 'Nothing to see here, mortal.' }: { message?: string }) {
  return (
    <div className="flex flex-col items-center justify-center py-16 gap-3">
      <p className="text-3xl opacity-40">🔮</p>
      <p className="text-text-muted text-sm">{message}</p>
    </div>
  )
}

export function CardSkeleton() {
  return (
    <div className="card p-5 space-y-3">
      <div className="flex justify-between">
        <Skeleton className="h-4 w-28" />
        <Skeleton className="h-4 w-16" />
      </div>
      <Skeleton className="h-8 w-36" />
      <Skeleton className="h-10 w-full" />
      <div className="flex justify-between">
        <Skeleton className="h-3 w-20" />
        <Skeleton className="h-3 w-16" />
      </div>
    </div>
  )
}
