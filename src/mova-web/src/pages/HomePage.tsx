import { Suspense, lazy } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Box, Button, Card, CardActions, CardContent, Container, Grid, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import Seo from '../components/Seo';

const HomeFeaturedSection = lazy(() => import('./HomeFeaturedSection'));

function HeroVisual() {
  return (
    <svg viewBox="0 0 200 200" width="100%" height="100%" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
      <circle cx="100" cy="100" r="90" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
      <path d="M100 10 V190" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
      <circle cx="100" cy="100" r="35" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
      <path d="M10 100 H190" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
      <path d="M25 25 Q100 100 175 25" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
      <path d="M25 175 Q100 100 175 175" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
      <path d="M25 25 Q100 100 25 175" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
      <path d="M175 25 Q100 100 175 175" stroke="currentColor" strokeWidth="2" strokeOpacity="0.6" />
    </svg>
  );
}

const ctaFocusOutline = {
  '&:focus-visible': {
    outline: '3px solid rgba(255,255,255,0.95)',
    outlineOffset: '3px'
  }
};

export default function HomePage() {
  const { t } = useTranslation();

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
    <Box>
      <Seo
        title={`${t('common.appName')} | ${t('seo.homeTitle')}`}
        description={t('seo.homeDescription')}
      />
      <Box
        component="section"
        aria-labelledby="hero-title"
        data-testid="hero"
        sx={{
          position: 'relative',
          overflow: 'hidden',
          mx: { xs: -2, sm: -3 },
          py: { xs: 8, sm: 10, md: 14 },
          background: 'linear-gradient(135deg, #0A3D3A 0%, #0F5E59 50%, #14877E 100%)',
          color: 'common.white',
          borderRadius: { xs: '0 0 1.5rem 1.5rem', md: '0 0 2rem 2rem' }
        }}
      >
        <Box
          aria-hidden="true"
          data-testid="hero-visual"
          sx={{
            position: 'absolute',
            inset: 0,
            opacity: 0.15,
            mixBlendMode: 'overlay',
            backgroundImage: 'radial-gradient(circle at 20% 30%, rgba(255,255,255,0.35) 0%, transparent 35%), radial-gradient(circle at 80% 70%, rgba(255,255,255,0.25) 0%, transparent 40%), radial-gradient(circle at 50% 100%, rgba(255,255,255,0.15) 0%, transparent 50%)'
          }}
        />
        <Box
          aria-hidden="true"
          sx={{
            position: 'absolute',
            top: { xs: '5%', md: '10%' },
            right: { xs: '-10%', sm: '-5%', md: '5%' },
            width: { xs: 180, sm: 260, md: 340 },
            height: { xs: 180, sm: 260, md: 340 },
            opacity: 0.12,
            color: 'common.white',
            pointerEvents: 'none'
          }}
        >
          <HeroVisual />
        </Box>
        <Container maxWidth="md" sx={{ position: 'relative', zIndex: 1 }}>
          <Stack spacing={{ xs: 3, md: 4 }} alignItems={{ xs: 'flex-start', md: 'center' }} textAlign={{ xs: 'left', md: 'center' }}>
            <Typography component="p" variant="overline" sx={{ letterSpacing: 3, opacity: 0.9, fontWeight: 600 }}>
              {t('home.hero.overline')}
            </Typography>
            <Typography id="hero-title" component="h1" variant="h2" sx={{ fontWeight: 900, fontSize: { xs: '2.5rem', sm: '3rem', md: '4.5rem' }, lineHeight: 1.1 }}>
              {t('home.hero.title')}
            </Typography>
            <Typography variant="h6" sx={{ maxWidth: 680, fontWeight: 400, opacity: 0.93, fontSize: { xs: '1.1rem', md: '1.35rem' } }}>
              {t('home.hero.subtitle')}
            </Typography>
            <Stack
              direction={{ xs: 'column', sm: 'row' }}
              spacing={2}
              useFlexGap
              flexWrap="wrap"
              justifyContent="center"
              sx={{ pt: 1 }}
            >
              <Button
                component={RouterLink}
                to="/login?intent=user"
                variant="contained"
                size="large"
                sx={{
                  px: 3,
                  py: 1.5,
                  fontWeight: 700,
                  bgcolor: 'common.white',
                  color: 'primary.dark',
                  '&:hover': { bgcolor: 'grey.100' },
                  ...ctaFocusOutline
                }}
              >
                {t('home.hero.play')}
              </Button>
              <Button
                component={RouterLink}
                to="/login?intent=complex"
                variant="outlined"
                size="large"
                sx={{
                  px: 3,
                  py: 1.5,
                  fontWeight: 700,
                  color: 'common.white',
                  borderColor: 'rgba(255,255,255,0.7)',
                  '&:hover': { borderColor: 'common.white', bgcolor: 'rgba(255,255,255,0.08)' },
                  ...ctaFocusOutline
                }}
              >
                {t('home.hero.manage')}
              </Button>
              <Button
                component={RouterLink}
                to="/complexes"
                variant="contained"
                size="large"
                sx={{
                  px: 3,
                  py: 1.5,
                  fontWeight: 700,
                  bgcolor: 'rgba(255,255,255,0.12)',
                  color: 'common.white',
                  border: '1px solid rgba(255,255,255,0.3)',
                  '&:hover': { bgcolor: 'rgba(255,255,255,0.2)' },
                  ...ctaFocusOutline
                }}
              >
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

      <Suspense fallback={
        <Box component="section" aria-labelledby="featured-title" sx={{ bgcolor: 'grey.100', py: { xs: 7, md: 10 } }}>
          <Container maxWidth="lg">
            <Stack spacing={2} alignItems="center" textAlign="center" sx={{ mb: 5 }}>
              <Skeleton variant="text" width={200} height={40} />
              <Skeleton variant="text" width={300} height={24} />
            </Stack>
            <Grid container spacing={3}>
              {[0, 1, 2].map((index) => <Grid key={index} size={{ xs: 12, sm: 6, md: 4 }}><Skeleton variant="rounded" height={190} /></Grid>)}
            </Grid>
          </Container>
        </Box>
      }>
        <HomeFeaturedSection />
      </Suspense>

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
