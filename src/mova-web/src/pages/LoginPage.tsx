import { Box, Typography } from '@mui/material';
import { GoogleLoginButton } from '../features/auth/GoogleLoginButton';

export default function LoginPage() {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3, mt: 8 }}>
      <Typography variant="h4">Welcome to Mova</Typography>
      <Typography variant="body1">Sign in with your Google account to continue.</Typography>
      <GoogleLoginButton />
    </Box>
  );
}
