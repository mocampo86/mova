import { Outlet } from 'react-router-dom';
import { Box, Toolbar } from '@mui/material';
import AppHeader from './AppHeader';

export default function AuthLayout() {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppHeader />
      <Toolbar />
      <Box component="main" sx={{ flexGrow: 1 }}>
        <Outlet />
      </Box>
    </Box>
  );
}
