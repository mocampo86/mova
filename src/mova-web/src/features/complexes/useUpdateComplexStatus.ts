import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../services/apiClient';
import { useAuth } from '../auth/useAuth';
import type { SportsComplex } from './complexTypes';

interface UpdateComplexStatusRequest {
  status: string;
}

export function useUpdateComplexStatus(complexId: string) {
  const queryClient = useQueryClient();
  const { accessToken } = useAuth();

  return useMutation<SportsComplex, Error, string>({
    mutationFn: async (status) => {
      if (!accessToken) {
        throw new Error('You must be logged in to update the complex status.');
      }

      const request: UpdateComplexStatusRequest = { status };

      return apiClient<SportsComplex>(`/api/v1/complexes/${complexId}/status`, {
        method: 'PATCH',
        body: JSON.stringify(request)
      }, accessToken);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['complex-dashboard', complexId] });
      await queryClient.invalidateQueries({ queryKey: ['active-complex', complexId] });
      await queryClient.invalidateQueries({ queryKey: ['active-complexes'] });
    }
  });
}
