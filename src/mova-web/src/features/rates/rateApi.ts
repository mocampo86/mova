import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type {
  CourtRate,
  CreateCourtRateRequest,
  PriceQuote,
  UpdateCourtRateRequest,
  UpdateCourtRateStatusRequest
} from './rateTypes';

export function useCourtRates(complexId: string, courtId: string) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['court-rates', complexId, courtId],
    queryFn: () =>
      apiClient<CourtRate[]>(
        `/api/v1/complexes/${complexId}/courts/${courtId}/rates`,
        {},
        accessToken ?? undefined
      ),
    enabled: Boolean(complexId && courtId && accessToken)
  });
}

export function useCreateCourtRate(complexId: string, courtId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<CourtRate, Error, CreateCourtRateRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to manage rates.');
      }

      return apiClient<CourtRate>(
        `/api/v1/complexes/${complexId}/courts/${courtId}/rates`,
        { method: 'POST', body: JSON.stringify(request) },
        accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['court-rates', complexId, courtId] });
      queryClient.invalidateQueries({ queryKey: ['price-quote', complexId, courtId] });
    }
  });
}

export function useUpdateCourtRate(complexId: string, courtId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<CourtRate, Error, { rateId: string; request: UpdateCourtRateRequest }>({
    mutationFn: async ({ rateId, request }) => {
      if (!accessToken) {
        throw new Error('You must be logged in to manage rates.');
      }

      return apiClient<CourtRate>(
        `/api/v1/complexes/${complexId}/courts/${courtId}/rates/${rateId}`,
        { method: 'PUT', body: JSON.stringify(request) },
        accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['court-rates', complexId, courtId] });
      queryClient.invalidateQueries({ queryKey: ['price-quote', complexId, courtId] });
    }
  });
}

export function useUpdateCourtRateStatus(complexId: string, courtId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<CourtRate, Error, { rateId: string; request: UpdateCourtRateStatusRequest }>({
    mutationFn: async ({ rateId, request }) => {
      if (!accessToken) {
        throw new Error('You must be logged in to manage rates.');
      }

      return apiClient<CourtRate>(
        `/api/v1/complexes/${complexId}/courts/${courtId}/rates/${rateId}/status`,
        { method: 'PATCH', body: JSON.stringify(request) },
        accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['court-rates', complexId, courtId] });
      queryClient.invalidateQueries({ queryKey: ['price-quote', complexId, courtId] });
    }
  });
}

export async function getPriceQuote(
  complexId: string,
  courtId: string,
  startAt: string,
  endAt: string,
  accessToken?: string
): Promise<PriceQuote> {
  const params = new URLSearchParams({ startAt, endAt });
  return apiClient<PriceQuote>(
    `/api/v1/complexes/${complexId}/courts/${courtId}/price-quote?${params}`,
    {},
    accessToken
  );
}

export function usePriceQuote(
  complexId: string,
  courtId: string,
  startAt: string | null | undefined,
  endAt: string | null | undefined
) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['price-quote', complexId, courtId, startAt, endAt],
    queryFn: () => getPriceQuote(complexId, courtId, startAt!, endAt!, accessToken ?? undefined),
    enabled: Boolean(complexId && courtId && startAt && endAt)
  });
}
