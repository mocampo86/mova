import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  completeComplexAdminProfile,
  type CompleteComplexAdminRequest,
  type CompleteComplexAdminResponse
} from '../../services/authApi';
import { useAuth } from '../auth/useAuth';

export function useCompleteComplexAdminProfile() {
  const navigate = useNavigate();
  const { accessToken, login } = useAuth();

  return useMutation<CompleteComplexAdminResponse, Error, CompleteComplexAdminRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to complete your profile.');
      }

      return completeComplexAdminProfile(request, accessToken);
    },
    onSuccess: (response) => {
      login(response.accessToken, response.requiresProfileCompletion);
      navigate(`/admin/complex/${response.complexId}`, { replace: true });
    }
  });
}
