import { useQuery } from '@tanstack/react-query'
import { transactionsApi } from '@/services/api'
import type { TransactionFilter } from '@/types/api'

export function useTransactions(filter: TransactionFilter = {}) {
  return useQuery({
    queryKey: ['transactions', filter],
    queryFn: () => transactionsApi.getAll(filter),
    staleTime: 30_000,
    placeholderData: (prev) => prev,
  })
}
