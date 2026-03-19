import { create } from 'zustand'
import type { TransactionFilter } from '@/types/api'

interface DashboardState {
  // Transaction filter state
  filter: TransactionFilter
  setFilter: (patch: Partial<TransactionFilter>) => void
  resetFilter: () => void

  // Selected account for detail view
  selectedAccountId: string | null
  setSelectedAccountId: (id: string | null) => void

  // Heimdall chat panel
  chatOpen: boolean
  setChatOpen: (open: boolean) => void

  // Bifrost import modal
  bifrostOpen: boolean
  setBifrostOpen: (open: boolean) => void

  // Page routing
  currentView: 'dashboard' | 'settings'
  setCurrentView: (view: 'dashboard' | 'settings') => void
}

const DEFAULT_FILTER: TransactionFilter = {
  page: 1,
  pageSize: 25,
}

export const useDashboardStore = create<DashboardState>((set) => ({
  filter: DEFAULT_FILTER,
  setFilter: (patch) =>
    set((s) => ({ filter: { ...s.filter, ...patch, page: patch.page ?? 1 } })),
  resetFilter: () => set({ filter: DEFAULT_FILTER }),

  selectedAccountId: null,
  setSelectedAccountId: (id) => set({ selectedAccountId: id }),

  chatOpen: false,
  setChatOpen: (open) => set({ chatOpen: open }),

  bifrostOpen: false,
  setBifrostOpen: (open) => set({ bifrostOpen: open }),

  currentView: 'dashboard',
  setCurrentView: (view) => set({ currentView: view }),
}))
