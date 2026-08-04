import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  completeProfile as completeProfileApi,
  type CompleteProfileRequest,
  type UserInfo
} from '../../services/usersApi';
import { useAuth } from '../auth/useAuth';

export function useCompleteProfile() {
  const navigate = useNavigate();
  const { accessToken, completeProfile } = useAuth();

  return useMutation<UserInfo, Error, CompleteProfileRequest>({
    mutationFn: async (request) => {
      if (!accessToken) {
        throw new Error('You must be logged in to complete your profile.');
      }

      return completeProfileApi(request, accessToken);
    },
    onSuccess: () => {
      completeProfile();
      navigate('/user', { replace: true });
    }
  });
}
