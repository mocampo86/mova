import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { PagedResult } from '../complexes/complexTypes';

export interface FavoriteComplex {
  id: string;
  name: string;
  description: string;
  address: string;
  city: string;
  status: string;
  sports: string[];
  createdAt: string;
}

export const favoritesKeys = {
  ids: ['favorite-complex-ids'] as const,
  list: (page: number, pageSize: number) => ['favorite-complexes', page, pageSize] as const,
  lists: ['favorite-complexes'] as const
};

export function useFavoriteComplexIds() {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: favoritesKeys.ids,
    queryFn: () => apiClient<{ items: string[] }>('/api/v1/users/me/favorites/ids', {}, accessToken ?? undefined),
    enabled: Boolean(accessToken)
  });
}

export function useFavoriteComplexes(page = 1, pageSize = 20) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: favoritesKeys.list(page, pageSize),
    queryFn: () =>
      apiClient<PagedResult<FavoriteComplex>>(
        `/api/v1/users/me/favorites?page=${page}&pageSize=${pageSize}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(accessToken)
  });
}

export function useAddFavoriteComplex() {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (complexId: string) =>
      apiClient<null>(`/api/v1/users/me/favorites/${complexId}`, { method: 'PUT' }, accessToken ?? undefined),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: favoritesKeys.ids });
      void queryClient.invalidateQueries({ queryKey: favoritesKeys.lists });
    }
  });
}

export function useRemoveFavoriteComplex() {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (complexId: string) =>
      apiClient<null>(`/api/v1/users/me/favorites/${complexId}`, { method: 'DELETE' }, accessToken ?? undefined),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: favoritesKeys.ids });
      void queryClient.invalidateQueries({ queryKey: favoritesKeys.lists });
    }
  });
}
