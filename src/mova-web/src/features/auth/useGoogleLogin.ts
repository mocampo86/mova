import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { googleLogin } from '../../services/authApi';
import { useAuth } from './useAuth';

export function useGoogleLogin() {
  const navigate = useNavigate();
  const { login } = useAuth();

  return useMutation({
    mutationFn: googleLogin,
    onSuccess: (response) => {
      login(response.accessToken, response.requiresProfileCompletion);

      if (response.requiresProfileCompletion) {
        navigate('/complete-profile', { replace: true });
      } else {
        navigate('/user', { replace: true });
      }
    }
  });
}
