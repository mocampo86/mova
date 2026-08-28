import { Outlet } from 'react-router-dom';
import { Box, Container, Toolbar } from '@mui/material';
import AppHeader from './AppHeader';

export default function PublicLayout() {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <AppHeader />
      <Toolbar />
      <Box component="main" sx={{ flexGrow: 1 }}>
        <Container maxWidth="lg">
          <Outlet />
        </Container>
      </Box>
    </Box>
  );
}
