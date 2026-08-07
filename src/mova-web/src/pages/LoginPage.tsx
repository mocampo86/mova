import { Box, Typography } from '@mui/material';
import { useSearchParams, Link as RouterLink } from 'react-router-dom';
import { GoogleLoginButton } from '../features/auth/GoogleLoginButton';

export default function LoginPage() {
  const [searchParams] = useSearchParams();
  const intent = searchParams.get('intent') === 'complex' ? 'complex' : 'user';

  const isComplex = intent === 'complex';

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3, mt: 8, px: 2 }}>
      <Typography variant="h4">{isComplex ? 'Administra tu complejo' : 'Welcome to Mova'}</Typography>
      <Typography variant="body1" textAlign="center">
        {isComplex
          ? 'Sign in with your Google account to register and manage your sports complex.'
          : 'Sign in with your Google account to continue.'}
      </Typography>
      <GoogleLoginButton intent={intent} />
      <Typography variant="body2" color="text.secondary">
        {isComplex ? (
          <>Want to book a court? <RouterLink to="/login?intent=user">Sign in as a player</RouterLink></>
        ) : (
          <>Own a complex? <RouterLink to="/login?intent=complex">Sign in as an owner</RouterLink></>
        )}
      </Typography>
    </Box>
  );
}
