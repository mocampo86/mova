import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { PagedResult } from '../complexes/complexTypes';
import type {
  CancelReservationRequest,
  CreateMyRecurringReservationRequest,
  CreateRecurringReservationForCustomerRequest,
  CreateMyReservationRequest,
  CreateReservationRequest,
  RecurringReservation,
  Reservation,
  ReservationListFilters,
  UpdateReservationStatusRequest,
  UserReservationsFilters
} from './reservationTypes';

function getUtcOffsetMinutes(date: string): number {
  return new Date(`${date}T00:00`).getTimezoneOffset();
}

export function useReservations(
  complexId: string,
  filters: ReservationListFilters,
  enabled = true
) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize),
    sort: filters.sort ?? 'startAt:desc'
  });

  if (filters.courtId) {
    params.set('courtId', filters.courtId);
  }

  if (filters.status && filters.status !== 'All') {
    params.set('status', filters.status);
  }

  if (filters.date) {
    params.set('date', filters.date);
    params.set('utcOffsetMinutes', String(getUtcOffsetMinutes(filters.date)));
  }

  return useQuery({
    queryKey: ['reservations', complexId, filters],
    queryFn: () =>
      apiClient<PagedResult<Reservation>>(
        `/api/v1/complexes/${complexId}/reservations?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && accessToken) && enabled
  });
}

export function useMyReservations(filters: UserReservationsFilters) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize)
  });

  return useQuery({
    queryKey: ['my-reservations', filters],
    queryFn: () =>
      apiClient<PagedResult<Reservation>>(
        `/api/v1/users/me/reservations?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(accessToken)
  });
}

export function useMyReservationHistory(filters: UserReservationsFilters) {
  const { accessToken } = useAuth();
  const params = new URLSearchParams({
    page: String(filters.page + 1),
    pageSize: String(filters.pageSize)
  });

  return useQuery({
    queryKey: ['my-reservation-history', filters],
    queryFn: () =>
      apiClient<PagedResult<Reservation>>(
        `/api/v1/users/me/reservations/history?${params}`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(accessToken)
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

export async function createMyReservation(
  complexId: string,
  request: CreateMyReservationRequest,
  accessToken: string
): Promise<Reservation> {
  return apiClient<Reservation>(
    `/api/v1/complexes/${complexId}/reservations/me`,
    {
      method: 'POST',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useCreateMyReservation(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<Reservation, Error, CreateMyReservationRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to create a reservation.');
      }

      return createMyReservation(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['court-availability', complexId] });
      queryClient.invalidateQueries({ queryKey: ['reservations', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}

export async function createMyRecurringReservation(
  complexId: string,
  request: CreateMyRecurringReservationRequest,
  accessToken: string
): Promise<RecurringReservation> {
  return apiClient<RecurringReservation>(
    `/api/v1/complexes/${complexId}/recurring-reservations/me`,
    {
      method: 'POST',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useCreateMyRecurringReservation(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<RecurringReservation, Error, CreateMyRecurringReservationRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to create a recurring reservation.');
      }

      return createMyRecurringReservation(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      queryClient.invalidateQueries({ queryKey: ['reservations', complexId] });
      queryClient.invalidateQueries({ queryKey: ['court-availability', complexId] });
    }
  });
}

export async function createRecurringReservationForCustomer(
  complexId: string,
  request: CreateRecurringReservationForCustomerRequest,
  accessToken: string
): Promise<RecurringReservation> {
  return apiClient<RecurringReservation>(
    `/api/v1/complexes/${complexId}/recurring-reservations`,
    {
      method: 'POST',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useCreateRecurringReservationForCustomer(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<RecurringReservation, Error, CreateRecurringReservationForCustomerRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to create a recurring reservation for a customer.');
      }

      return createRecurringReservationForCustomer(complexId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations', complexId] });
      queryClient.invalidateQueries({ queryKey: ['court-availability', complexId] });
      queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
    }
  });
}

export async function cancelMyReservation(
  reservationId: string,
  request: CancelReservationRequest,
  accessToken: string
): Promise<Reservation> {
  return apiClient<Reservation>(
    `/api/v1/users/me/reservations/${reservationId}/cancel`,
    {
      method: 'PATCH',
      body: JSON.stringify(request)
    },
    accessToken
  );
}

export function useCancelMyReservation() {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<Reservation, Error, { reservationId: string; request: CancelReservationRequest }>({
    mutationFn: async ({ reservationId, request }) => {
      if (!accessToken) {
        throw new Error('You must be logged in to cancel a reservation.');
      }

      return cancelMyReservation(reservationId, request, accessToken);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      queryClient.invalidateQueries({ queryKey: ['my-reservation-history'] });
      queryClient.invalidateQueries({ queryKey: ['court-availability'] });
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
      queryClient.invalidateQueries({ queryKey: ['court-availability', complexId] });
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
      queryClient.invalidateQueries({ queryKey: ['court-availability', complexId] });
    }
  });
}
