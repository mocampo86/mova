import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { PagedResult } from '../complexes/complexTypes';
import type {
  CancelReservationRequest,
  CreateReservationRequest,
  Reservation,
  ReservationListFilters,
  UpdateReservationStatusRequest
} from './reservationTypes';

export function useReservations(complexId: string, filters: ReservationListFilters) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize),
    sort: 'startAt:desc'
  });

  if (filters.courtId) {
    params.set('courtId', filters.courtId);
  }

  if (filters.status && filters.status !== 'All') {
    params.set('status', filters.status);
  }

  if (filters.date) {
    params.set('date', filters.date);
  }

  return useQuery({
    queryKey: ['reservations', complexId, filters],
    queryFn: () =>
      apiClient<PagedResult<Reservation>>(
        `/api/v1/complexes/${complexId}/reservations?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && accessToken)
  });
}

export async function createReservation(
  complexId: string,
  request: CreateReservationRequest,
  accessToken: string
): Promise<Reservation> {
  return apiClient<Reservation>(
    `/api/v1/complexes/${complexId}/reservations`,
    {
      method: 'POST',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useCreateReservation(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<Reservation, Error, CreateReservationRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to create a reservation.');
      }

      return createReservation(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}

export async function cancelReservation(
  complexId: string,
  reservationId: string,
  request: CancelReservationRequest,
  accessToken: string
): Promise<Reservation> {
  return apiClient<Reservation>(
    `/api/v1/complexes/${complexId}/reservations/${reservationId}/cancel`,
    {
      method: 'PATCH',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useCancelReservation(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<Reservation, Error, { reservationId: string; request: CancelReservationRequest }>({
    mutationFn: async ({ reservationId, request }) => {
      if (!accessToken) {
        throw new Error('You must be logged in to cancel a reservation.');
      }

      return cancelReservation(complexId, reservationId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}

export async function updateReservationStatus(
  complexId: string,
  reservationId: string,
  request: UpdateReservationStatusRequest,
  accessToken: string
): Promise<Reservation> {
  return apiClient<Reservation>(
    `/api/v1/complexes/${complexId}/reservations/${reservationId}/status`,
    {
      method: 'PATCH',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useUpdateReservationStatus(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<Reservation, Error, { reservationId: string; request: UpdateReservationStatusRequest }>({
    mutationFn: async ({ reservationId, request }) => {
      if (!accessToken) {
        throw new Error('You must be logged in to update the reservation status.');
      }

      return updateReservationStatus(complexId, reservationId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}
