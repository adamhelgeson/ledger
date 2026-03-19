import { cn } from '@/lib/utils'

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {}

export function Input({ className, ...props }: InputProps) {
  return (
    <input
      className={cn(
        'flex h-9 w-full rounded-lg border border-border bg-bg-elevated px-3 py-1',
        'text-sm text-text-primary placeholder:text-text-muted',
        'focus:outline-none focus:ring-2 focus:ring-gold/40 focus:border-gold/50',
        'transition-all duration-150',
        className,
      )}
      {...props}
    />
  )
}
