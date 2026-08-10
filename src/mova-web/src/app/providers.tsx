import type { ReactNode } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { BrowserRouter } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { I18nextProvider } from 'react-i18next';
import AuthProvider from '../features/auth/AuthProvider';
import i18n from '../i18n';
import queryClient from './queryClient';
import theme from './theme';

interface AppProvidersProps {
  children: ReactNode;
}

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? '';

export default function AppProviders({ children }: AppProvidersProps) {
  return (
    <I18nextProvider i18n={i18n}>
      <GoogleOAuthProvider clientId={googleClientId}>
        <QueryClientProvider client={queryClient}>
          <ThemeProvider theme={theme}>
            <CssBaseline />
            <BrowserRouter>
              <AuthProvider>{children}</AuthProvider>
            </BrowserRouter>
          </ThemeProvider>
        </QueryClientProvider>
      </GoogleOAuthProvider>
    </I18nextProvider>
  );
}
