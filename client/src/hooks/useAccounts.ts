import { useQuery } from '@tanstack/react-query'
import { accountsApi } from '@/services/api'

export function useAccounts() {
  return useQuery({
    queryKey: ['accounts'],
    queryFn: () => accountsApi.getAll(),
    staleTime: 60_000,
  })
}

export function useAccount(id: string | null) {
  return useQuery({
    queryKey: ['accounts', id],
    queryFn: () => accountsApi.getById(id!),
    enabled: !!id,
    staleTime: 60_000,
  })
}
