import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/** Debounce a function call. */
export function debounce<T extends (...args: Parameters<T>) => void>(fn: T, ms: number): T {
  let timer: ReturnType<typeof setTimeout>
  return ((...args: Parameters<T>) => {
    clearTimeout(timer)
    timer = setTimeout(() => fn(...args), ms)
  }) as T
}

/** Clamp a number between min and max. */
export function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max)
}

/** Generate a deterministic mock sparkline series for visual demo purposes.
 *  Uses the account ID hash as a seed so the same account always gets the same shape. */
export function mockSparkline(baseValue: number, accountId: string, points = 14): number[] {
  let seed = accountId.split('').reduce((acc, c) => acc + c.charCodeAt(0), 0)
  const rand = () => {
    seed = (seed * 1664525 + 1013904223) & 0x7fffffff
    return (seed / 0x7fffffff) * 2 - 1
  }

  const series: number[] = []
  let current = baseValue * 0.88
  for (let i = 0; i < points; i++) {
    current = current + rand() * baseValue * 0.025
    series.push(Math.max(0, current))
  }
  // Last point = actual current balance
  series[series.length - 1] = baseValue
  return series
}

/** Map a category string to a Tailwind colour class for badges. */
export const CATEGORY_COLORS: Record<string, string> = {
  Groceries: 'bg-emerald-900/40 text-emerald-400 border-emerald-700/40',
  Dining: 'bg-orange-900/40 text-orange-400 border-orange-700/40',
  Subscriptions: 'bg-purple-900/40 text-purple-400 border-purple-700/40',
  Gas: 'bg-yellow-900/40 text-yellow-400 border-yellow-700/40',
  Utilities: 'bg-blue-900/40 text-blue-400 border-blue-700/40',
  Shopping: 'bg-pink-900/40 text-pink-400 border-pink-700/40',
  Health: 'bg-red-900/40 text-red-400 border-red-700/40',
  Transport: 'bg-cyan-900/40 text-cyan-400 border-cyan-700/40',
  Income: 'bg-green-900/40 text-green-400 border-green-700/40',
  Housing: 'bg-indigo-900/40 text-indigo-400 border-indigo-700/40',
  Transfers: 'bg-slate-700/40 text-slate-400 border-slate-600/40',
  Other: 'bg-zinc-800/40 text-zinc-400 border-zinc-700/40',
}

export function getCategoryColor(category: string): string {
  return CATEGORY_COLORS[category] ?? CATEGORY_COLORS['Other']
}
