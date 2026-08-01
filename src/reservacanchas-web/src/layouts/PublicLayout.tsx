import { Outlet } from 'react-router-dom';
import { Container, Typography } from '@mui/material';

export default function PublicLayout() {
  return (
    <Container maxWidth="lg">
      <Typography variant="h4" component="header" gutterBottom>
        Reserva Canchas
      </Typography>
      <main>
        <Outlet />
      </main>
    </Container>
  );
}
