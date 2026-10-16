import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: { main: '#2563EB', dark: '#1D4ED8', contrastText: '#FFFFFF' },
    success: { main: '#16A34A' },
    warning: { main: '#F59E0B' },
    error: { main: '#DC2626' },
    background: { default: '#F5F7FA', paper: '#FFFFFF' },
    text: { primary: '#172B4D', secondary: '#64748B' },
    divider: '#E2E8F0'
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily: '"Aptos", "Segoe UI", sans-serif',
    h4: { fontWeight: 750, letterSpacing: '-0.02em' },
    h5: { fontWeight: 750, letterSpacing: '-0.015em' },
    button: { fontWeight: 700, textTransform: 'none' }
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: { backgroundColor: '#F5F7FA' },
        '*:focus-visible': { outline: '3px solid #93C5FD', outlineOffset: '2px' }
      }
    },
    MuiPaper: {
      styleOverrides: {
        root: { boxShadow: '0 2px 8px rgba(23, 43, 77, 0.06)' }
      }
    },
    MuiButton: {
      styleOverrides: {
        root: { borderRadius: 8, boxShadow: 'none' },
        contained: { '&:hover': { boxShadow: '0 4px 10px rgba(37, 99, 235, 0.2)' } }
      }
    }
  }
});

export default theme;
