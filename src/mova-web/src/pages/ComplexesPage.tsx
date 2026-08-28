import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Alert, Button, Card, CardContent, Container, Grid, Pagination, Stack, TextField, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useActiveComplexes } from '../features/complexes/complexApi';

export default function ComplexesPage() {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [submittedSearch, setSubmittedSearch] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading, isError } = useActiveComplexes(submittedSearch, page);

  return (
    <Container component="main" maxWidth="lg" sx={{ py: 6 }}>
      <Stack spacing={3}>
        <div>
          <Typography component="h1" variant="h3" sx={{ fontWeight: 800 }}>{t('complexes.title')}</Typography>
          <Typography color="text.secondary">{t('complexes.subtitle')}</Typography>
        </div>
        <Stack component="form" direction={{ xs: 'column', sm: 'row' }} spacing={2} onSubmit={(event) => { event.preventDefault(); setSubmittedSearch(search); setPage(1); }}>
          <TextField fullWidth label={t('complexes.searchPlaceholder')} value={search} onChange={(event) => setSearch(event.target.value)} inputProps={{ maxLength: 100 }} />
          <Button type="submit" variant="contained" sx={{ minWidth: 120 }}>{t('complexes.searchButton')}</Button>
        </Stack>
        {isLoading && <Typography>{t('complexes.loading')}</Typography>}
        {isError && <Alert severity="error">{t('complexes.error')}</Alert>}
        {!isLoading && !isError && data?.items.length === 0 && <Alert severity="info">{t('complexes.empty')}</Alert>}
        <Grid container spacing={3}>
          {data?.items.map((complex) => (
            <Grid key={complex.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
                <CardContent>
                  <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>{complex.name}</Typography>
                  <Typography color="text.secondary" sx={{ mt: 1 }}>{complex.city}{t('common.formatSeparator')}{complex.address}</Typography>
                  <Typography sx={{ mt: 2 }}>{complex.description || t('common.noDescription')}</Typography>
                  <Button component={RouterLink} to={`/complexes/${complex.id}`} sx={{ mt: 2 }}>{t('common.view')}</Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
        {!isLoading && !isError && data && data.totalPages > 1 && (
          <Stack alignItems="center">
            <Pagination count={data.totalPages} page={page} onChange={(_, value) => setPage(value)} color="primary" size="large" />
          </Stack>
        )}
      </Stack>
    </Container>
  );
}
