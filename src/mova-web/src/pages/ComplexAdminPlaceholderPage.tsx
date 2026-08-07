import { Container, Typography } from '@mui/material';

export default function ComplexAdminPlaceholderPage() {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h5" component="h2" gutterBottom>
        Section under construction
      </Typography>
      <Typography color="text.secondary">
        This admin section is not yet available.
      </Typography>
    </Container>
  );
}
