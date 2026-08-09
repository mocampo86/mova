import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { PagedResult } from '../complexes/complexTypes';
import type { Court } from '../complexes/complexTypes';
import type { CourtListFilters, CreateCourtRequest, UpdateCourtStatusRequest } from './courtTypes';

export function useCourts(complexId: string, filters: CourtListFilters) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page),
    pageSize: String(filters.pageSize),
    status: filters.status
  });

  if (filters.sportId) {
    params.set('sportId', filters.sportId);
  }

  return useQuery({
    queryKey: ['courts', complexId, filters],
    queryFn: () =>
      apiClient<PagedResult<Court>>(
        `/api/v1/complexes/${complexId}/courts?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && accessToken)
  });
}

export async function updateCourtStatus(
  complexId: string,
  courtId: string,
  request: UpdateCourtStatusRequest,
  accessToken: string
): Promise<Court> {
  return apiClient<Court>(
    `/api/v1/complexes/${complexId}/courts/${courtId}/status`,
    {
      method: 'PATCH',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useUpdateCourtStatus(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<Court, Error, { courtId: string; request: UpdateCourtStatusRequest }>({
    mutationFn: async ({ courtId, request }) => {
      if (!accessToken) {
        throw new Error('You must be logged in to update the court status.');
      }

      return updateCourtStatus(complexId, courtId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['courts', complexId] });
      queryClient.invalidateQueries({ queryKey: ['active-courts', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}

export async function createCourt(
  complexId: string,
  request: CreateCourtRequest,
  accessToken: string
): Promise<Court> {
  return apiClient<Court>(
    `/api/v1/complexes/${complexId}/courts`,
    {
      method: 'POST',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useCreateCourt(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<Court, Error, CreateCourtRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to create a court.');
      }

      return createCourt(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['courts', complexId] });
      queryClient.invalidateQueries({ queryKey: ['active-courts', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}
