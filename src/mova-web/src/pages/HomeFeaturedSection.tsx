import { Link as RouterLink } from 'react-router-dom';
import { Alert, Box, Button, Card, CardActions, CardContent, Container, Grid, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useActiveComplexes } from '../features/complexes/complexApi';

export default function HomeFeaturedSection() {
  const { t } = useTranslation();
  const { data: featuredComplexes, isLoading: isFeaturedLoading, isError: isFeaturedError } = useActiveComplexes('', 1, 3);

  return (
    <Box component="section" aria-labelledby="featured-title" sx={{ bgcolor: 'grey.100', py: { xs: 7, md: 10 } }}>
      <Container maxWidth="lg">
        <Stack spacing={2} alignItems="center" textAlign="center" sx={{ mb: 5 }}>
          <Typography id="featured-title" component="h2" variant="h4" sx={{ fontWeight: 700 }}>{t('home.featured.title')}</Typography>
          <Typography color="text.secondary">{t('home.featured.subtitle')}</Typography>
        </Stack>
        {isFeaturedLoading && <Grid container spacing={3}>{[0, 1, 2].map((index) => <Grid key={index} size={{ xs: 12, sm: 6, md: 4 }}><Skeleton variant="rounded" height={190} /></Grid>)}</Grid>}
        {isFeaturedError && <Alert severity="error">{t('home.featured.error')}</Alert>}
        {!isFeaturedLoading && !isFeaturedError && featuredComplexes?.items.length === 0 && <Alert severity="info">{t('home.featured.empty')}</Alert>}
        <Grid container spacing={3}>
          {featuredComplexes?.items.map((complex) => (
            <Grid key={complex.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
                <CardContent>
                  <Typography component="h3" variant="h6" sx={{ fontWeight: 700 }}>{complex.name}</Typography>
                  <Typography color="text.secondary" sx={{ mt: 1 }}>{complex.city}{t('common.formatSeparator')}{complex.address}</Typography>
                  <Typography sx={{ mt: 2 }}>{complex.description || t('common.noDescription')}</Typography>
                </CardContent>
                <CardActions sx={{ px: 2, pb: 2 }}><Button component={RouterLink} to={`/complexes/${complex.id}`}>{t('home.featured.action')}</Button></CardActions>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  );
}
