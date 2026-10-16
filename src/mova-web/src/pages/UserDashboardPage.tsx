import { Link as RouterLink } from 'react-router-dom';
import { Alert, Box, Button, Card, CardActionArea, CardContent, Chip, Grid, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useActiveComplexes } from '../features/complexes/complexApi';
import { useFavoriteComplexes } from '../features/favorites/favoritesApi';
import { useUserDashboard } from '../features/users/useUserDashboard';
import { getReservationStatusKey } from '../features/reservations/reservationStatus';
import UserContentContainer from '../layouts/UserContentContainer';

function dateTime(value: string) {
  return new Intl.DateTimeFormat(undefined, { weekday: 'short', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(value));
}

function CalendarIcon() {
  return <Box component="svg" viewBox="0 0 24 24" aria-hidden="true" sx={{ width: 23, height: 23, fill: 'none', stroke: 'currentColor', strokeWidth: 1.8 }}><rect x="4" y="5" width="16" height="15" rx="2" /><path d="M8 3v4m8-4v4M4 10h16" /></Box>;
}

export default function UserDashboardPage() {
  const { t } = useTranslation();
  const dashboard = useUserDashboard();
  const complexes = useActiveComplexes('', 1, 4);
  const favorites = useFavoriteComplexes(1, 3);
  const nextReservation = dashboard.data?.upcomingReservations.items[0];
  const nextComplex = complexes.data?.items.find((complex) => complex.id === nextReservation?.complexId);
  const name = dashboard.data?.user.fullName || dashboard.data?.user.email || '';

  return <UserContentContainer><Stack spacing={{ xs: 3.5, md: 4 }}>
    {dashboard.isError && <Alert severity="error">{t('dashboard.errorMessage')}</Alert>}
    <Stack spacing={0.35}>
      <Typography component="h1" variant="h4" sx={{ fontWeight: 850, fontSize: { xs: '1.75rem', sm: '2.125rem' } }}>{dashboard.isLoading ? <Skeleton width={210} /> : t('dashboard.welcome', { name })}</Typography>
      <Typography color="text.secondary" sx={{ fontSize: { xs: '1rem', sm: '1.05rem' } }}>{t('dashboard.homeQuestion')}</Typography>
    </Stack>

    <Button component={RouterLink} to="/complexes" variant="contained" size="large" startIcon={<CalendarIcon />} sx={{ alignSelf: 'flex-start', minHeight: 52, px: { xs: 2, sm: 2.5 }, fontWeight: 800, borderRadius: 2, maxWidth: '100%' }}>
      {t('dashboard.browseComplexes')}
    </Button>

    <Box component="section" aria-labelledby="upcoming-reservations-title"><Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'center' }} spacing={{ xs: 0.5, sm: 1 }} sx={{ mb: 1.25, minWidth: 0 }}><Typography id="upcoming-reservations-title" component="h2" variant="h6" sx={{ fontWeight: 800 }}>{t('dashboard.upcomingTitle')}</Typography>{nextReservation && <Button component={RouterLink} to="/user/reservations" size="small">{t('dashboard.viewAllReservations')}</Button>}</Stack>
      {dashboard.isLoading ? <Skeleton variant="rounded" height={150} /> : nextReservation ? <Card variant="outlined" sx={{ borderRadius: 3, overflow: 'hidden', boxShadow: '0 5px 16px rgba(23,43,77,.06)' }}><CardActionArea component={RouterLink} to="/user/reservations"><CardContent sx={{ p: { xs: 2, sm: 2.5 } }}><Stack direction="row" spacing={1.5} alignItems="flex-start"><Box sx={{ width: 44, height: 44, borderRadius: 2, display: 'grid', placeItems: 'center', bgcolor: 'primary.light', color: 'primary.dark', flexShrink: 0 }}><CalendarIcon /></Box><Box sx={{ minWidth: 0, flexGrow: 1 }}><Stack direction="row" justifyContent="space-between" alignItems="flex-start" gap={1} sx={{ minWidth: 0 }}><Typography fontWeight={800} sx={{ minWidth: 0, overflowWrap: 'anywhere' }}>{nextReservation.courtName}</Typography><Chip sx={{ flexShrink: 0 }} label={t(`status.${getReservationStatusKey(nextReservation.status)}`)} color="primary" size="small" /></Stack>{nextComplex && <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25, overflowWrap: 'anywhere' }}>{nextComplex.name}</Typography>}<Typography color="text.secondary" variant="body2" sx={{ mt: 0.75 }}>{dateTime(nextReservation.startAt)}</Typography></Box></Stack></CardContent></CardActionArea></Card> : <Card variant="outlined" sx={{ borderRadius: 3 }}><CardContent sx={{ p: 2.25 }}><Typography fontWeight={800}>{t('dashboard.noUpcomingReservations')}</Typography><Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{t('dashboard.noUpcomingDescription')}</Typography><Button component={RouterLink} to="/complexes" sx={{ mt: 1, px: 0 }}>{t('dashboard.browseComplexes')}</Button></CardContent></Card>}
    </Box>

    <Box component="section" aria-labelledby="favorites-title"><Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'center' }} spacing={{ xs: 0.5, sm: 1 }} sx={{ mb: 1.25, minWidth: 0 }}><Typography id="favorites-title" component="h2" variant="h6" sx={{ fontWeight: 800 }}>{t('favorites.sectionTitle')}</Typography>{favorites.data && favorites.data.totalItems > 0 && <Button component={RouterLink} to="/user/favorites" size="small">{t('favorites.viewAll')}</Button>}</Stack>
      {favorites.isLoading ? <Skeleton variant="rounded" height={110} /> : favorites.isError ? <Alert severity="error">{t('favorites.loadError')}</Alert> : favorites.data && favorites.data.items.length > 0 ? <Stack spacing={1.5}>{favorites.data.items.map((favorite) => <Card key={favorite.id} variant="outlined" sx={{ borderRadius: 3, overflow: 'hidden', boxShadow: '0 3px 12px rgba(23,43,77,.04)' }}><CardActionArea component={RouterLink} to={`/complexes/${favorite.id}`}><CardContent sx={{ p: 2 }}><Stack direction="row" spacing={1.5} alignItems="center" sx={{ minWidth: 0 }}><Box aria-hidden="true" sx={{ width: 40, height: 40, borderRadius: 2, display: 'grid', placeItems: 'center', bgcolor: 'grey.100', color: 'primary.main', fontWeight: 900, flexShrink: 0 }}>{favorite.name.slice(0, 1).toUpperCase()}</Box><Box sx={{ minWidth: 0, flexGrow: 1 }}><Typography fontWeight={800} sx={{ overflowWrap: 'anywhere' }}>{favorite.name}</Typography>{(favorite.city || favorite.address) && <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25, overflowWrap: 'anywhere' }}>{[favorite.city, favorite.address].filter(Boolean).join(' · ')}</Typography>}</Box>{favorite.status !== 'Active' && <Chip label={t('favorites.unavailable')} size="small" color="warning" variant="outlined" sx={{ flexShrink: 0 }} />}</Stack></CardContent></CardActionArea></Card>)}</Stack> : <Card variant="outlined" sx={{ borderRadius: 3 }}><CardContent sx={{ p: 2.25 }}><Typography fontWeight={800}>{t('favorites.emptyTitle')}</Typography><Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{t('favorites.emptyInvite')}</Typography><Button component={RouterLink} to="/complexes" sx={{ mt: 1, px: 0 }}>{t('favorites.explore')}</Button></CardContent></Card>}
    </Box>

    <Box component="section" aria-labelledby="where-to-play-title"><Stack direction={{ xs: 'column', sm: 'row' }} justifyContent="space-between" alignItems={{ xs: 'flex-start', sm: 'center' }} spacing={{ xs: 0.5, sm: 1 }} sx={{ mb: 1.25, minWidth: 0 }}><Box sx={{ minWidth: 0, maxWidth: '100%' }}><Typography id="where-to-play-title" component="h2" variant="h6" sx={{ fontWeight: 800 }}>{t('dashboard.whereToPlay')}</Typography><Typography variant="body2" color="text.secondary">{t('dashboard.exploreComplexesDescription')}</Typography></Box><Button component={RouterLink} to="/complexes" size="small">{t('common.view')}</Button></Stack>
      {complexes.isError ? <Alert severity="error">{t('complexes.error')}</Alert> : <Grid container spacing={2}>{complexes.isLoading ? [0, 1].map((key) => <Grid key={key} size={{ xs: 12, sm: 6 }}><Skeleton variant="rounded" height={176} /></Grid>) : complexes.data?.items.map((complex) => <Grid key={complex.id} size={{ xs: 12, sm: 6 }}><Card variant="outlined" sx={{ height: '100%', borderRadius: 3, overflow: 'hidden', boxShadow: '0 3px 12px rgba(23,43,77,.04)' }}><CardActionArea component={RouterLink} to={`/complexes/${complex.id}`} sx={{ height: '100%' }}><CardContent sx={{ p: 2.25 }}><Stack direction="row" spacing={1.5}><Box aria-hidden="true" sx={{ width: 40, height: 40, borderRadius: 2, display: 'grid', placeItems: 'center', bgcolor: 'grey.100', color: 'primary.main', fontWeight: 900, flexShrink: 0 }}>{complex.name.slice(0, 1).toUpperCase()}</Box><Box sx={{ minWidth: 0 }}><Typography fontWeight={800} sx={{ overflowWrap: 'anywhere' }}>{complex.name}</Typography>{(complex.city || complex.address) && <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25, overflowWrap: 'anywhere' }}>{[complex.city, complex.address].filter(Boolean).join(' · ')}</Typography>}</Box></Stack>{complex.description && <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5, overflowWrap: 'anywhere' }}>{complex.description}</Typography>}<Typography variant="body2" color="primary.main" sx={{ mt: 1.5, fontWeight: 700 }}>{t('dashboard.viewAvailability')}</Typography></CardContent></CardActionArea></Card></Grid>)}</Grid>}
    </Box>
  </Stack></UserContentContainer>;
}
