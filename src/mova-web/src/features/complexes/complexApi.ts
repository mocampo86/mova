import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type {
  BusinessHours,
  CancellationPolicy,
  ComplexDashboard,
  Court,
  CourtAvailabilitySlot,
  PagedResult,
  Sport,
  SportsComplex,
  UpdateBusinessHoursRequest,
  UpdateCancellationPolicyRequest,
  UpdateComplexRequest,
  UpdateRecurringReservationSettingsRequest
} from './complexTypes';

export function useActiveComplexes(search: string, page = 1) {
  const params = new URLSearchParams({ page: String(page), pageSize: '12' });
  if (search.trim()) params.set('search', search.trim());

  return useQuery({
    queryKey: ['active-complexes', search, page],
    queryFn: () => apiClient<PagedResult<SportsComplex>>(`/api/v1/complexes?${params}`)
  });
}

export function useActiveComplex(id: string) {
  return useQuery({
    queryKey: ['active-complex', id],
    queryFn: () => apiClient<SportsComplex>(`/api/v1/complexes/${id}`),
    enabled: Boolean(id)
  });
}

export function useAdminComplex(id: string) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['admin-complex', id],
    queryFn: () =>
      apiClient<SportsComplex>(`/api/v1/complexes/${id}/admin`, {}, accessToken ?? undefined),
    enabled: Boolean(id && accessToken)
  });
}

export function useActiveCourts(complexId: string, sportId?: string) {
  const params = new URLSearchParams({ page: '1', pageSize: '100' });
  if (sportId) params.set('sportId', sportId);

  return useQuery({
    queryKey: ['active-courts', complexId, sportId],
    queryFn: () => apiClient<PagedResult<Court>>(`/api/v1/complexes/${complexId}/courts?${params}`),
    enabled: Boolean(complexId)
  });
}

export function useSports() {
  return useQuery({
    queryKey: ['active-sports'],
    queryFn: () => apiClient<Sport[]>('/api/v1/sports')
  });
}

export function useCourtAvailability(complexId: string, courtId: string, date: string, utcOffsetMinutes: number = 0) {
  const params = new URLSearchParams({
    courtId,
    date,
    utcOffsetMinutes: utcOffsetMinutes.toString()
  });

  return useQuery({
    queryKey: ['court-availability', complexId, courtId, date, utcOffsetMinutes],
    queryFn: () => apiClient<CourtAvailabilitySlot[]>(`/api/v1/complexes/${complexId}/availability?${params}`),
    enabled: Boolean(complexId && courtId && date)
  });
}

export function useComplexDashboard(complexId: string) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['complex-dashboard', complexId],
    queryFn: () =>
      apiClient<ComplexDashboard>(`/api/v1/complexes/${complexId}/dashboard`, {}, accessToken ?? undefined),
    enabled: Boolean(complexId && accessToken)
  });
}

export async function updateComplex(
  complexId: string,
  request: UpdateComplexRequest,
  accessToken: string
): Promise<SportsComplex> {
  return apiClient<SportsComplex>(
    `/api/v1/complexes/${complexId}`,
    {
      method: 'PUT',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useUpdateComplex(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<SportsComplex, Error, UpdateComplexRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to update the complex.');
      }

      return updateComplex(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['active-complex', complexId] });
      queryClient.invalidateQueries({ queryKey: ['admin-complex', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
      queryClient.invalidateQueries({ queryKey: ['active-complexes'] });
    }
  });
}

export function useBusinessHours(complexId: string) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['business-hours', complexId],
    queryFn: () =>
      apiClient<BusinessHours[]>(`/api/v1/complexes/${complexId}/business-hours`, {}, accessToken ?? undefined),
    enabled: Boolean(complexId && accessToken)
  });
}

export function useUpdateBusinessHours(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<BusinessHours[], Error, UpdateBusinessHoursRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to update business hours.');
      }

      return apiClient<BusinessHours[]>(
        `/api/v1/complexes/${complexId}/business-hours`,
        {
          method: 'PUT',
          body: JSON.stringify(request)
        },
        accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['business-hours', complexId] });
    }
  });
}

export function useCancellationPolicy(complexId: string) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['cancellation-policy', complexId],
    queryFn: () =>
      apiClient<CancellationPolicy>(
        `/api/v1/complexes/${complexId}/configuration/cancellation-policy`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && accessToken)
  });
}

export async function updateCancellationPolicy(
  complexId: string,
  request: UpdateCancellationPolicyRequest,
  accessToken: string
): Promise<CancellationPolicy> {
  return apiClient<CancellationPolicy>(
    `/api/v1/complexes/${complexId}/configuration/cancellation-policy`,
    {
      method: 'PUT',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useUpdateCancellationPolicy(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<CancellationPolicy, Error, UpdateCancellationPolicyRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to update the cancellation policy.');
      }

      return updateCancellationPolicy(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cancellation-policy', complexId] });
    }
  });
}

export function useRecurringReservationSettings(complexId: string) {
  return useActiveComplex(complexId);
}

export async function updateRecurringReservationSettings(
  complexId: string,
  request: UpdateRecurringReservationSettingsRequest,
  accessToken: string
): Promise<SportsComplex> {
  return apiClient<SportsComplex>(
    `/api/v1/complexes/${complexId}/configuration/recurring-reservations`,
    {
      method: 'PUT',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useUpdateRecurringReservationSettings(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<SportsComplex, Error, UpdateRecurringReservationSettingsRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to update the recurring reservation settings.');
      }

      return updateRecurringReservationSettings(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['active-complex', complexId] });
      queryClient.invalidateQueries({ queryKey: ['active-complexes'] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}
