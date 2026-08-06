import { Link as RouterLink, useParams } from 'react-router-dom';
import { Alert, Button, Card, CardContent, Container, Grid, Stack, Typography } from '@mui/material';
import { useActiveComplex, useActiveCourts } from '../features/complexes/complexApi';

export default function ComplexDetailPage() {
  const { complexId = '' } = useParams();
  const complex = useActiveComplex(complexId);
  const courts = useActiveCourts(complexId);

  if (complex.isLoading || courts.isLoading) return <Container sx={{ py: 6 }}><Typography>Loading complex…</Typography></Container>;
  if (complex.isError || !complex.data) return <Container sx={{ py: 6 }}><Alert severity="error">This active complex could not be found.</Alert></Container>;

  return (
    <Container component="main" maxWidth="lg" sx={{ py: 6 }}>
      <Stack spacing={3}>
        <Button component={RouterLink} to="/complexes" sx={{ alignSelf: 'flex-start' }}>← All complexes</Button>
        <div>
          <Typography component="h1" variant="h3" sx={{ fontWeight: 800 }}>{complex.data.name}</Typography>
          <Typography color="text.secondary">{complex.data.city} · {complex.data.address}</Typography>
          <Typography sx={{ mt: 2 }}>{complex.data.description}</Typography>
        </div>
        <Typography component="h2" variant="h5" sx={{ fontWeight: 700 }}>Active courts</Typography>
        {courts.isError && <Alert severity="error">Courts could not be loaded.</Alert>}
        {!courts.isError && courts.data?.items.length === 0 && <Alert severity="info">No active courts are currently published.</Alert>}
        <Grid container spacing={3}>
          {courts.data?.items.map((court) => (
            <Grid key={court.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card variant="outlined" sx={{ borderRadius: 3 }}><CardContent>
                <Typography component="h3" variant="h6" sx={{ fontWeight: 700 }}>{court.name}</Typography>
                <Typography color="text.secondary">{court.indoor ? 'Indoor' : 'Outdoor'}{court.surfaceType ? ` · ${court.surfaceType}` : ''}</Typography>
                {court.description && <Typography sx={{ mt: 1 }}>{court.description}</Typography>}
              </CardContent></Card>
            </Grid>
          ))}
        </Grid>
      </Stack>
    </Container>
  );
}
