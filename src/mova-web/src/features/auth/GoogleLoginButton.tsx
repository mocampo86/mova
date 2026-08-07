import { GoogleLogin, type CredentialResponse } from '@react-oauth/google';
import { Box, CircularProgress, Typography } from '@mui/material';
import { useGoogleLogin } from './useGoogleLogin';

interface GoogleLoginButtonProps {
  intent?: 'user' | 'complex';
}

export function GoogleLoginButton({ intent = 'user' }: GoogleLoginButtonProps) {
  const { mutate, isPending, error } = useGoogleLogin(intent);

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
      {error && <Typography color="error">{error.message}</Typography>}
    </Box>
  );
}
