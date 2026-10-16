import { Outlet } from 'react-router-dom';
import { Box, Container, Toolbar } from '@mui/material';
import AppHeader from './AppHeader';

export default function PublicLayout() {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100dvh', width: '100%', minWidth: 0, bgcolor: 'background.default' }}>
      <AppHeader />
      <Toolbar />
      <Box component="main" sx={{ flexGrow: 1, minWidth: 0, width: '100%' }}>
        <Container maxWidth="lg" sx={{ px: { xs: 2, sm: 3 } }}>
          <Outlet />
        </Container>
      </Box>
    </Box>
  );
}
