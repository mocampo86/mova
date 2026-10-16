import { GoogleLogin, type CredentialResponse } from '@react-oauth/google';
import { Box, CircularProgress, Typography } from '@mui/material';
import ApiErrorMessage from '../../components/ApiErrorMessage';
import { useGoogleLogin } from './useGoogleLogin';

interface GoogleLoginButtonProps {
  intent?: 'user' | 'complex';
  returnTo?: string | null;
}

export function GoogleLoginButton({ intent = 'user', returnTo }: GoogleLoginButtonProps) {
  const { mutate, isPending, error } = useGoogleLogin(intent, returnTo);

  const handleSuccess = (credentialResponse: CredentialResponse) => {
    if (credentialResponse.credential) {
      mutate({ idToken: credentialResponse.credential });
    }
  };

  const handleError = () => {
    // Google login errors are surfaced by the user closing the prompt or failing to authenticate.
    // No credential is returned in those cases.
  };

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
      <GoogleLogin onSuccess={handleSuccess} onError={handleError} />
      {isPending && <CircularProgress size={24} />}
      {error && <Typography color="error"><ApiErrorMessage error={error} /></Typography>}
    </Box>
  );
}
