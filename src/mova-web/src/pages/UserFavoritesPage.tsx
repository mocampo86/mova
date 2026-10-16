import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Grid,
  Pagination,
  Skeleton,
  Stack,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import Seo from '../components/Seo';
import FavoriteToggleButton from '../features/favorites/FavoriteToggleButton';
import { useFavoriteComplexes } from '../features/favorites/favoritesApi';
import UserContentContainer from '../layouts/UserContentContainer';

const PAGE_SIZE = 12;

export default function UserFavoritesPage() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);
  const { data, isLoading, isError, refetch } = useFavoriteComplexes(page, PAGE_SIZE);

  return (
    <UserContentContainer sx={{ py: { xs: 3, sm: 6 } }}>
      <Seo
        title={`${t('common.appName')} | ${t('favorites.title')}`}
        description={t('favorites.subtitle')}
      />
      <Stack spacing={{ xs: 2.5, sm: 3 }}>
        <Box sx={{ minWidth: 0 }}>
          <Typography component="h1" sx={{ fontWeight: 800, fontSize: { xs: '1.5rem', sm: '2rem' } }}>
            {t('favorites.title')}
          </Typography>
          <Typography color="text.secondary" sx={{ mt: 0.5 }}>
            {t('favorites.subtitle')}
          </Typography>
        </Box>

        {isLoading && (
          <Grid container spacing={{ xs: 2, sm: 3 }}>
            {[0, 1, 2].map((key) => (
              <Grid key={key} size={{ xs: 12, sm: 6, md: 4 }}>
                <Skeleton variant="rounded" height={190} />
              </Grid>
            ))}
          </Grid>
        )}

        {isError && (
          <Alert
            severity="error"
            action={
              <Button color="inherit" size="small" onClick={() => refetch()}>
                {t('common.retry')}
              </Button>
            }
          >
            {t('favorites.loadError')}
          </Alert>
        )}

        {!isLoading && !isError && data?.items.length === 0 && (
          <Card variant="outlined" sx={{ borderRadius: 3 }}>
            <CardContent sx={{ p: { xs: 3, sm: 4 }, textAlign: 'center' }}>
              <Box
                component="svg"
                viewBox="0 0 24 24"
                aria-hidden="true"
                sx={{ width: 48, height: 48, color: 'text.secondary', fill: 'none', stroke: 'currentColor', strokeWidth: 1.5, strokeLinecap: 'round', strokeLinejoin: 'round', mb: 1 }}
              >
                <path d="M12 20.5 4.7 13a4.9 4.9 0 0 1 0-6.8 4.7 4.7 0 0 1 6.7 0l.6.6.6-.6a4.7 4.7 0 0 1 6.7 0 4.9 4.9 0 0 1 0 6.8z" />
              </Box>
              <Typography fontWeight={800}>{t('favorites.emptyTitle')}</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, maxWidth: 420, mx: 'auto' }}>
                {t('favorites.emptyDescription')}
              </Typography>
              <Button component={RouterLink} to="/complexes" variant="contained" sx={{ mt: 2, fontWeight: 700 }}>
                {t('favorites.explore')}
              </Button>
            </CardContent>
          </Card>
        )}

        <Grid container spacing={{ xs: 2, sm: 3 }}>
          {data?.items.map((complex) => {
            const isAvailable = complex.status === 'Active';
            return (
              <Grid key={complex.id} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card
                  variant="outlined"
                  sx={{ height: '100%', borderRadius: 3, overflow: 'hidden', boxShadow: '0 3px 12px rgba(23,43,77,.04)', opacity: isAvailable ? 1 : 0.72 }}
                >
                  <CardContent sx={{ p: { xs: 2, sm: 2.5 }, display: 'flex', flexDirection: 'column', height: '100%', minWidth: 0 }}>
                    <Stack direction="row" spacing={1.5} sx={{ minWidth: 0 }}>
                      <Box
                        aria-hidden="true"
                        sx={{ width: 44, height: 44, borderRadius: 2, display: 'grid', placeItems: 'center', bgcolor: 'grey.100', color: 'primary.main', fontWeight: 900, fontSize: '1.1rem', flexShrink: 0 }}
                      >
                        {complex.name.slice(0, 1).toUpperCase()}
                      </Box>
                      <Box sx={{ minWidth: 0, flexGrow: 1 }}>
                        <Typography component="h2" sx={{ fontWeight: 800, overflowWrap: 'anywhere' }}>
                          {complex.name}
                        </Typography>
                        {(complex.city || complex.address) && (
                          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25, overflowWrap: 'anywhere' }}>
                            {[complex.city, complex.address].filter(Boolean).join(' · ')}
                          </Typography>
                        )}
                      </Box>
                      <FavoriteToggleButton complexId={complex.id} />
                    </Stack>
                    <Stack direction="row" spacing={0.75} sx={{ mt: 1.5, flexWrap: 'wrap', gap: 0.75 }}>
                      {complex.sports.map((sport) => (
                        <Chip key={sport} label={sport} size="small" variant="outlined" />
                      ))}
                      {!isAvailable && (
                        <Chip label={t('favorites.unavailable')} size="small" color="warning" variant="outlined" />
                      )}
                    </Stack>
                    <Box sx={{ flexGrow: 1, minHeight: 16 }} />
                    <Button
                      component={RouterLink}
                      to={`/complexes/${complex.id}`}
                      variant="contained"
                      size="small"
                      disabled={!isAvailable}
                      sx={{ alignSelf: { xs: 'stretch', sm: 'flex-start' }, minHeight: 40, fontWeight: 700 }}
                    >
                      {t('favorites.viewCourts')}
                    </Button>
                  </CardContent>
                </Card>
              </Grid>
            );
          })}
        </Grid>

        {!isLoading && !isError && data && data.totalPages > 1 && (
          <Stack alignItems="center">
            <Pagination count={data.totalPages} page={page} onChange={(_, value) => setPage(value)} color="primary" size="large" />
          </Stack>
        )}
      </Stack>
    </UserContentContainer>
  );
}
