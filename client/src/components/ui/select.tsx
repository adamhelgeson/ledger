import { cn } from '@/lib/utils'
import { ChevronDown } from 'lucide-react'

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string
}

export function Select({ className, label, children, ...props }: SelectProps) {
  return (
    <div className="relative">
      <select
        className={cn(
          'h-9 w-full appearance-none rounded-lg border border-border bg-bg-elevated',
          'pl-3 pr-8 text-sm text-text-primary',
          'focus:outline-none focus:ring-2 focus:ring-gold/40 focus:border-gold/50',
          'transition-all duration-150 cursor-pointer',
          className,
        )}
        {...props}
      >
        {children}
      </select>
      <ChevronDown
        size={14}
        className="pointer-events-none absolute right-2.5 top-1/2 -translate-y-1/2 text-text-muted"
      />
    </div>
  )
}
