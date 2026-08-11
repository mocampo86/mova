import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { googleLogin } from '../../services/authApi';
import { mapJwtToUser } from '../../shared/utils/jwtParser';
import { useAuth } from './useAuth';

export function useGoogleLogin(intent: 'user' | 'complex' = 'user') {
  const navigate = useNavigate();
  const { login } = useAuth();

  return useMutation({
    mutationFn: googleLogin,
    onSuccess: (response) => {
      login(response.accessToken, response.requiresProfileCompletion);

      if (intent === 'complex') {
        const user = mapJwtToUser(response.accessToken);
        const complexAssociation = user.complexes?.find(
          (association) => association.role === 'ComplexAdmin'
        );

        if (complexAssociation) {
          navigate(`/admin/complex/${complexAssociation.complexId}`, { replace: true });
        } else {
          navigate('/complete-complex-admin', { replace: true });
        }
      } else if (response.requiresProfileCompletion) {
        navigate('/complete-profile', { replace: true });
      } else {
        navigate('/complexes', { replace: true });
      }
    }
  });
}
