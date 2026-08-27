import { useQuery } from '@tanstack/react-query';
import { getMyBlockStatus } from '../../services/usersApi';
import { useAuth } from '../auth/useAuth';

export function useMyBlockStatus(complexId: string) {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['my-block-status', complexId],
    queryFn: () => {
      if (!accessToken) {
        throw new Error('You must be logged in to view your block status.');
      }

      return getMyBlockStatus(complexId, accessToken);
    },
    enabled: Boolean(complexId && accessToken)
  });
}
