import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Alert, Button, Card, CardContent, Container, Grid, Stack, TextField, Typography } from '@mui/material';
import { useActiveComplexes } from '../features/complexes/complexApi';

export default function ComplexesPage() {
  const [search, setSearch] = useState('');
  const [submittedSearch, setSubmittedSearch] = useState('');
  const { data, isLoading, isError } = useActiveComplexes(submittedSearch);

  return (
    <Container component="main" maxWidth="lg" sx={{ py: 6 }}>
      <Stack spacing={3}>
        <div>
          <Typography component="h1" variant="h3" sx={{ fontWeight: 800 }}>Find a sports complex</Typography>
          <Typography color="text.secondary">Search active complexes by name, city, or address.</Typography>
        </div>
        <Stack component="form" direction={{ xs: 'column', sm: 'row' }} spacing={2} onSubmit={(event) => { event.preventDefault(); setSubmittedSearch(search); }}>
          <TextField fullWidth label="Search complexes" value={search} onChange={(event) => setSearch(event.target.value)} inputProps={{ maxLength: 100 }} />
          <Button type="submit" variant="contained" sx={{ minWidth: 120 }}>Search</Button>
        </Stack>
        {isLoading && <Typography>Loading complexes…</Typography>}
        {isError && <Alert severity="error">Complexes could not be loaded. Please try again.</Alert>}
        {!isLoading && !isError && data?.items.length === 0 && <Alert severity="info">No active complexes match your search.</Alert>}
        <Grid container spacing={3}>
          {data?.items.map((complex) => (
            <Grid key={complex.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
                <CardContent>
                  <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>{complex.name}</Typography>
                  <Typography color="text.secondary" sx={{ mt: 1 }}>{complex.city} · {complex.address}</Typography>
                  <Typography sx={{ mt: 2 }}>{complex.description || 'Explore courts and availability at this complex.'}</Typography>
                  <Button component={RouterLink} to={`/complexes/${complex.id}`} sx={{ mt: 2 }}>View courts</Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Stack>
    </Container>
  );
}
