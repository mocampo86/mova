import { useEffect, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Alert, Box, Button, Card, CardContent, Grid, IconButton, InputAdornment, Pagination, Skeleton, Stack, TextField, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import Seo from '../components/Seo';
import { useActiveComplexes } from '../features/complexes/complexApi';
import UserContentContainer from '../layouts/UserContentContainer';

function SearchIcon() {
  return <Box component="svg" viewBox="0 0 24 24" aria-hidden="true" sx={{ width: 20, height: 20, fill: 'none', stroke: 'currentColor', strokeWidth: 1.8, strokeLinecap: 'round' }}><circle cx="11" cy="11" r="6.5" /><path d="m20 20-4-4" /></Box>;
}

export default function ComplexesPage() {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [submittedSearch, setSubmittedSearch] = useState('');
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, refetch } = useActiveComplexes(submittedSearch, page);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setSubmittedSearch(search);
      setPage(1);
    }, 350);
    return () => window.clearTimeout(timeout);
  }, [search]);

  const clearSearch = () => {
    setSearch('');
    setSubmittedSearch('');
    setPage(1);
  };

  return (
    <UserContentContainer sx={{ py: { xs: 3, sm: 6 } }}>
      <Seo
        title={`${t('common.appName')} | ${t('seo.complexesTitle')}`}
        description={t('seo.complexesDescription')}
      />
      <Stack spacing={{ xs: 2.5, sm: 3 }}>
        <Box sx={{ minWidth: 0 }}>
          <Typography component="h1" sx={{ fontWeight: 800, fontSize: { xs: '1.5rem', sm: '2rem' } }}>{t('complexes.title')}</Typography>
          <Typography color="text.secondary" sx={{ mt: 0.5 }}>{t('complexes.subtitle')}</Typography>
        </Box>

        <Box component="form" role="search" onSubmit={(event) => { event.preventDefault(); setSubmittedSearch(search); setPage(1); }}>
          <TextField
            fullWidth
            type="search"
            label={t('complexes.searchLabel')}
            placeholder={t('complexes.searchPlaceholder')}
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            inputProps={{ maxLength: 100 }}
            InputProps={{
              startAdornment: <InputAdornment position="start"><SearchIcon /></InputAdornment>,
              endAdornment: search ? (
                <InputAdornment position="end">
                  <IconButton size="small" aria-label={t('complexes.clearSearch')} onClick={clearSearch} edge="end">
                    <Box component="svg" viewBox="0 0 24 24" aria-hidden="true" sx={{ width: 18, height: 18, fill: 'none', stroke: 'currentColor', strokeWidth: 1.8, strokeLinecap: 'round' }}><path d="M6 6l12 12M18 6 6 18" /></Box>
                  </IconButton>
                </InputAdornment>
              ) : undefined
            }}
            sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2.5, minHeight: 48, bgcolor: 'background.paper' } }}
          />
        </Box>

        <Box component="section" aria-labelledby="available-complexes-title" sx={{ minWidth: 0 }}>
          <Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'baseline' }} spacing={{ xs: 0.25, sm: 1 }} sx={{ mb: 1.5, minWidth: 0 }}>
            <Typography id="available-complexes-title" component="h2" variant="h6" sx={{ fontWeight: 800 }}>{t('complexes.availableTitle')}</Typography>
            {data && <Typography variant="body2" color="text.secondary">{t('complexes.availableCount', { count: data.totalItems })}</Typography>}
          </Stack>

          {isLoading && <Grid container spacing={{ xs: 2, sm: 3 }}>{[0, 1, 2].map((key) => <Grid key={key} size={{ xs: 12, sm: 6, md: 4 }}><Skeleton variant="rounded" height={190} /></Grid>)}</Grid>}

          {isError && (
            <Alert severity="error" action={<Button color="inherit" size="small" onClick={() => refetch()}>{t('common.retry')}</Button>}>
              {t('complexes.error')}
            </Alert>
          )}

          {!isLoading && !isError && data?.items.length === 0 && (
            <Card variant="outlined" sx={{ borderRadius: 3 }}>
              <CardContent sx={{ p: { xs: 2.5, sm: 3 }, textAlign: 'center' }}>
                <Typography fontWeight={700}>{t('complexes.empty')}</Typography>
                {submittedSearch && <Button size="small" onClick={clearSearch} sx={{ mt: 1 }}>{t('common.clear')}</Button>}
              </CardContent>
            </Card>
          )}

          <Grid container spacing={{ xs: 2, sm: 3 }}>
            {data?.items.map((complex) => (
              <Grid key={complex.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card variant="outlined" sx={{ height: '100%', borderRadius: 3, overflow: 'hidden', boxShadow: '0 3px 12px rgba(23,43,77,.04)' }}>
                  <CardContent sx={{ p: { xs: 2, sm: 2.5 }, display: 'flex', flexDirection: 'column', height: '100%', minWidth: 0 }}>
                    <Stack direction="row" spacing={1.5} sx={{ minWidth: 0 }}>
                      <Box aria-hidden="true" sx={{ width: 44, height: 44, borderRadius: 2, display: 'grid', placeItems: 'center', bgcolor: 'grey.100', color: 'primary.main', fontWeight: 900, fontSize: '1.1rem', flexShrink: 0 }}>
                        {complex.name.slice(0, 1).toUpperCase()}
                      </Box>
                      <Box sx={{ minWidth: 0 }}>
                        <Typography component="h3" sx={{ fontWeight: 800, overflowWrap: 'anywhere' }}>{complex.name}</Typography>
                        {(complex.city || complex.address) && (
                          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25, overflowWrap: 'anywhere' }}>
                            {[complex.city, complex.address].filter(Boolean).join(' · ')}
                          </Typography>
                        )}
                      </Box>
                    </Stack>
                    {complex.description && (
                      <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5, overflowWrap: 'anywhere', display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                        {complex.description}
                      </Typography>
                    )}
                    <Box sx={{ flexGrow: 1, minHeight: 16 }} />
                    <Button component={RouterLink} to={`/complexes/${complex.id}`} variant="contained" size="small" sx={{ alignSelf: { xs: 'stretch', sm: 'flex-start' }, minHeight: 40, fontWeight: 700 }}>
                      {t('dashboard.viewAvailability')}
                    </Button>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </Box>

        {!isLoading && !isError && data && data.totalPages > 1 && (
          <Stack alignItems="center">
            <Pagination count={data.totalPages} page={page} onChange={(_, value) => setPage(value)} color="primary" size="large" />
          </Stack>
        )}
      </Stack>
    </UserContentContainer>
  );
}
