import { Link as RouterLink } from 'react-router-dom';
import { Alert, Box, Button, Card, CardActions, CardContent, Container, Grid, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useActiveComplexes } from '../features/complexes/complexApi';

export default function HomePage() {
  const { t } = useTranslation();
  const { data: featuredComplexes, isLoading: isFeaturedLoading, isError: isFeaturedError } = useActiveComplexes('', 1);

  const steps = [
    {
      title: t('home.steps.searchTitle'),
      description: t('home.steps.searchDescription')
    },
    {
      title: t('home.steps.selectTitle'),
      description: t('home.steps.selectDescription')
    },
    {
      title: t('home.steps.bookTitle'),
      description: t('home.steps.bookDescription')
    }
  ];

  return (
    <Box component="main">
      <Box
        component="section"
        aria-labelledby="hero-title"
        sx={{
          py: { xs: 7, md: 12 },
          px: { xs: 2, md: 6 },
          borderRadius: 4,
          background: 'linear-gradient(135deg, #0f766e 0%, #115e59 100%)',
          color: 'common.white'
        }}
      >
        <Container maxWidth="md">
          <Stack spacing={3} alignItems={{ xs: 'flex-start', md: 'center' }} textAlign={{ xs: 'left', md: 'center' }}>
            <Typography component="p" variant="overline" sx={{ letterSpacing: 2, opacity: 0.85 }}>
              {t('home.hero.overline')}
            </Typography>
            <Typography id="hero-title" component="h1" variant="h2" sx={{ fontWeight: 800, fontSize: { xs: '2.5rem', md: '4rem' } }}>
              {t('home.hero.title')}
            </Typography>
            <Typography variant="h6" sx={{ maxWidth: 650, fontWeight: 400, opacity: 0.92 }}>
              {t('home.hero.subtitle')}
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Button component={RouterLink} to="/login?intent=user" variant="contained" size="large" color="secondary">
                {t('home.hero.play')}
              </Button>
              <Button component={RouterLink} to="/login?intent=complex" variant="outlined" size="large" sx={{ color: 'common.white', borderColor: 'rgba(255,255,255,.7)' }}>
                {t('home.hero.manage')}
              </Button>
              <Button component={RouterLink} to="/complexes" variant="outlined" size="large" sx={{ color: 'common.white', borderColor: 'rgba(255,255,255,.7)' }}>
                {t('home.hero.browse')}
              </Button>
            </Stack>
          </Stack>
        </Container>
      </Box>

      <Container id="how-it-works" component="section" aria-labelledby="steps-title" maxWidth="lg" sx={{ py: { xs: 7, md: 10 } }}>
        <Stack spacing={2} alignItems="center" textAlign="center" sx={{ mb: 5 }}>
          <Typography id="steps-title" component="h2" variant="h4" sx={{ fontWeight: 700 }}>
            {t('home.steps.title')}
          </Typography>
          <Typography color="text.secondary" sx={{ maxWidth: 620 }}>
            {t('home.steps.subtitle')}
          </Typography>
        </Stack>
        <Grid container spacing={3}>
          {steps.map((step) => (
            <Grid key={step.title} size={{ xs: 12, md: 4 }}>
              <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
                <CardContent sx={{ p: 3 }}>
                  <Typography component="h3" variant="h6" sx={{ fontWeight: 700, mb: 1 }}>
                    {step.title}
                  </Typography>
                  <Typography color="text.secondary">{step.description}</Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
        <Stack alignItems="center" sx={{ mt: 4 }}>
          <Button component={RouterLink} to="/complexes" variant="contained" size="large">
            {t('home.steps.action')}
          </Button>
        </Stack>
      </Container>

      <Container component="section" aria-labelledby="audiences-title" maxWidth="lg" sx={{ py: { xs: 7, md: 10 } }}>
        <Typography id="audiences-title" component="h2" variant="h4" sx={{ fontWeight: 700, textAlign: 'center', mb: 5 }}>
          {t('home.audiences.title')}
        </Typography>
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
              <CardContent sx={{ p: 4 }}>
                <Typography component="h3" variant="h5" sx={{ fontWeight: 700, mb: 1 }}>{t('home.audiences.playersTitle')}</Typography>
                <Typography color="text.secondary">{t('home.audiences.playersDescription')}</Typography>
              </CardContent>
              <CardActions sx={{ px: 3, pb: 3 }}>
                <Button component={RouterLink} to="/complexes">{t('home.audiences.playersAction')}</Button>
              </CardActions>
            </Card>
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
              <CardContent sx={{ p: 4 }}>
                <Typography component="h3" variant="h5" sx={{ fontWeight: 700, mb: 1 }}>{t('home.audiences.ownersTitle')}</Typography>
                <Typography color="text.secondary">{t('home.audiences.ownersDescription')}</Typography>
              </CardContent>
              <CardActions sx={{ px: 3, pb: 3 }}>
                <Button component={RouterLink} to="/login?intent=complex">{t('home.audiences.ownersAction')}</Button>
              </CardActions>
            </Card>
          </Grid>
        </Grid>
      </Container>

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
            {featuredComplexes?.items.slice(0, 3).map((complex) => (
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

      <Box component="section" aria-labelledby="cta-title" sx={{ py: 7, px: 2, textAlign: 'center' }}>
        <Stack spacing={2} alignItems="center">
          <Typography id="cta-title" component="h2" variant="h5" sx={{ fontWeight: 700 }}>
            {t('home.cta.title')}
          </Typography>
          <Typography color="text.secondary">{t('home.cta.subtitle')}</Typography>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
            <Button component={RouterLink} to="/login?intent=user" variant="contained" size="large">
              {t('home.cta.signIn')}
            </Button>
            <Button component={RouterLink} to="/login?intent=complex" variant="outlined" size="large">
              {t('home.cta.register')}
            </Button>
          </Stack>
        </Stack>
      </Box>
    </Box>
  );
}
