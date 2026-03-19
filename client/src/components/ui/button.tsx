import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const buttonVariants = cva(
  'inline-flex items-center justify-center gap-2 rounded-lg text-sm font-medium transition-all duration-150 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-gold/60 disabled:opacity-40 disabled:pointer-events-none',
  {
    variants: {
      variant: {
        default:
          'bg-gold text-bg-primary hover:bg-gold-light active:bg-gold-muted font-display tracking-wide',
        secondary:
          'bg-bg-elevated border border-border text-text-primary hover:border-gold/40 hover:text-gold',
        ghost: 'text-text-secondary hover:text-text-primary hover:bg-bg-elevated',
        destructive: 'bg-banished/10 border border-banished/40 text-banished hover:bg-banished/20',
        outline: 'border border-border bg-transparent text-text-primary hover:bg-bg-elevated',
      },
      size: {
        sm: 'h-7 px-2.5 text-xs',
        md: 'h-9 px-4',
        lg: 'h-11 px-6 text-base',
        icon: 'h-8 w-8',
      },
    },
    defaultVariants: { variant: 'secondary', size: 'md' },
  },
)

interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {}

export function Button({ className, variant, size, ...props }: ButtonProps) {
  return <button className={cn(buttonVariants({ variant, size }), className)} {...props} />
}
