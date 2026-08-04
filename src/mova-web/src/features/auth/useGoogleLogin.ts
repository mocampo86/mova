import { useMutation } from '@tanstack/react-query';
import { googleLogin } from '../../services/authApi';
import { useAuth } from './useAuth';

export function useGoogleLogin() {
  const { login } = useAuth();

  return useMutation({
    mutationFn: googleLogin,
    onSuccess: (response) => {
      login(response.accessToken);
    }
  });
}
