import { useQuery } from '@tanstack/react-query';
import { getUserDashboard } from '../../services/usersApi';
import { useAuth } from '../auth/useAuth';

export function useUserDashboard() {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ['user-dashboard'],
    queryFn: () => {
      if (!accessToken) {
        throw new Error('You must be logged in to view the dashboard.');
      }

      return getUserDashboard(accessToken);
    },
    enabled: Boolean(accessToken)
  });
}
