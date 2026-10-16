import { Link as RouterLink } from 'react-router-dom';
import { Alert, Box, Button, Card, CardActions, CardContent, Container, Grid, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { responsiveCtaSx } from '../shared/responsive';
import { useActiveComplexes } from '../features/complexes/complexApi';

export default function HomeFeaturedSection() {
  const { t } = useTranslation();
  const { data: featuredComplexes, isLoading, isError } = useActiveComplexes('', 1, 3);
  return (
    <Box component="section" aria-labelledby="featured-title" sx={{ py: { xs: 5, md: 9 } }}>
      <Container maxWidth="lg">
        <Stack spacing={1} sx={{ mb: { xs: 3, md: 4 } }}>
          <Typography id="featured-title" component="h2" variant="h4" sx={{ fontWeight: 800 }}>{t('home.featured.title')}</Typography>
          <Typography color="text.secondary">{t('home.featured.subtitle')}</Typography>
        </Stack>
        {isLoading && <Grid container spacing={2}>{[0, 1, 2].map((index) => <Grid key={index} size={{ xs: 12, sm: 6, md: 4 }}><Skeleton variant="rounded" height={210} /></Grid>)}</Grid>}
        {isError && <Alert severity="error">{t('home.featured.error')}</Alert>}
        {!isLoading && !isError && featuredComplexes?.items.length === 0 && <Alert severity="info">{t('home.featured.empty')}</Alert>}
        <Grid container spacing={2.5}>
          {featuredComplexes?.items.map((complex) => (
            <Grid key={complex.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card variant="outlined" sx={{ height: '100%', borderRadius: 3, display: 'flex', flexDirection: 'column' }}>
                <Box aria-hidden="true" sx={{ height: 88, bgcolor: 'primary.light', color: 'primary.dark', display: 'flex', alignItems: 'flex-end', p: 2, fontSize: '1.4rem', fontWeight: 900, letterSpacing: 1 }}>{complex.name.slice(0, 1).toUpperCase()}</Box>
                <CardContent sx={{ pb: 1, flexGrow: 1 }}>
                  <Typography component="h3" variant="h6" sx={{ fontWeight: 800 }}>{complex.name}</Typography>
                  {(complex.city || complex.address) && <Typography variant="body2" color="text.secondary" sx={{ mt: 0.75 }}>{[complex.city, complex.address].filter(Boolean).join(' · ')}</Typography>}
                  {complex.description && <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5 }}>{complex.description}</Typography>}
                </CardContent>
                <CardActions sx={{ px: 2, pb: 2 }}><Button component={RouterLink} to={'/complexes/' + complex.id} variant="outlined" fullWidth sx={{ minHeight: 44, ...responsiveCtaSx }}>{t('home.featured.action')}</Button></CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  );
}
