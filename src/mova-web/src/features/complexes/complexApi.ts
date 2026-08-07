import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { ComplexDashboard, Court, CourtAvailabilitySlot, PagedResult, Sport, SportsComplex } from './complexTypes';

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

export function useCourtAvailability(complexId: string, courtId: string, date: string) {
  const params = new URLSearchParams({ courtId, date });

  return useQuery({
    queryKey: ['court-availability', complexId, courtId, date],
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
