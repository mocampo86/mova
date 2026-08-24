import { useQueries } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import type { CourtAvailabilitySlot } from '../complexes/complexTypes';

function buildAvailabilityUrl(complexId: string, courtId: string, date: string): string {
  const params = new URLSearchParams({
    courtId,
    date
  });

  return `/api/v1/complexes/${complexId}/availability?${params}`;
}

export function useCourtAvailabilityForCourts(
  complexId: string,
  date: string,
  courtIds: string[],
  enabled: boolean
) {
  return useQueries({
    queries: enabled
      ? courtIds.map((courtId) => ({
          queryKey: ['court-availability', complexId, courtId, date] as const,
          queryFn: () =>
            apiClient<CourtAvailabilitySlot[]>(
              buildAvailabilityUrl(complexId, courtId, date)
            ),
          enabled: Boolean(complexId && date && courtId)
        }))
      : [],
    combine: (results) => ({
      data: Object.fromEntries(
        courtIds.map((id, index) => [id, results[index]?.data ?? []])
      ) as Record<string, CourtAvailabilitySlot[]>,
      isLoading: results.some((result) => result.isLoading),
      isError: results.some((result) => result.isError)
    })
  });
}
