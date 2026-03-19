import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const badgeVariants = cva(
  'inline-flex items-center gap-1 rounded-md border px-2 py-0.5 text-xs font-medium font-mono transition-colors',
  {
    variants: {
      variant: {
        default: 'border-border bg-bg-elevated text-text-secondary',
        gold: 'border-gold/40 bg-gold/10 text-gold',
        blue: 'border-blue-accent/40 bg-blue-accent/10 text-blue-accent',
        worthy: 'border-worthy/40 bg-worthy/10 text-worthy',
        unworthy: 'border-unworthy/40 bg-unworthy/10 text-unworthy',
        banished: 'border-banished/40 bg-banished/10 text-banished',
        debit: 'border-banished/30 bg-banished/10 text-banished',
        credit: 'border-worthy/30 bg-worthy/10 text-worthy',
      },
    },
    defaultVariants: { variant: 'default' },
  },
)

interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement>, VariantProps<typeof badgeVariants> {}

export function Badge({ className, variant, ...props }: BadgeProps) {
  return <span className={cn(badgeVariants({ variant }), className)} {...props} />
}
