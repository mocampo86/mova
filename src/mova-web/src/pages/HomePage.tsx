import { Suspense, lazy } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Box, Button, Card, CardContent, Container, Grid, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import Seo from '../components/Seo';
import HeroCarousel from '../components/HeroCarousel';
import { heroSlides } from './heroCarouselConfig';
import { responsiveCtaSx } from '../shared/responsive';

const HomeFeaturedSection = lazy(() => import('./HomeFeaturedSection'));

const stepIcons = ['1', '2', '3'];

export default function HomePage() {
  const { t } = useTranslation();
  const steps = [
    { title: t('home.steps.searchTitle'), description: t('home.steps.searchDescription') },
    { title: t('home.steps.selectTitle'), description: t('home.steps.selectDescription') },
    { title: t('home.steps.bookTitle'), description: t('home.steps.bookDescription') }
  ];

  return (
    <Box>
      <Seo title={`${t('common.appName')} | ${t('seo.homeTitle')}`} description={t('seo.homeDescription')} />
      <Box
        component="section"
        aria-labelledby="hero-title"
        data-testid="hero"
        sx={{
          position: 'relative', overflow: 'hidden', mx: { xs: -2, sm: -3 },
          minHeight: { xs: 300, md: 500 }, py: { xs: 5, sm: 6, md: 13 },
          bgcolor: { xs: '#F5F7FA', md: 'primary.dark' }, color: { xs: 'text.primary', md: 'common.white' },
          borderRadius: { xs: '0 0 24px 24px', md: '0 0 32px 32px' }
        }}
      >
        <Box sx={{ display: { xs: 'none', md: 'block' } }}>
          <HeroCarousel slides={heroSlides} interval={6000} data-testid="hero-visual" />
        </Box>
        <Container maxWidth="md" sx={{ position: 'relative', zIndex: 3 }}>
          <Stack spacing={{ xs: 2, md: 3 }} alignItems={{ xs: 'flex-start', md: 'center' }} textAlign={{ xs: 'left', md: 'center' }}>
            <Typography component="p" variant="overline" sx={{ letterSpacing: 2, color: { xs: 'primary.main', md: 'inherit' }, fontWeight: 800 }}>
              {t('home.hero.overline')}
            </Typography>
            <Typography id="hero-title" component="h1" variant="h2" sx={{ fontWeight: 900, fontSize: { xs: '2.25rem', sm: '2.75rem', md: '4.25rem' }, lineHeight: 1.08, maxWidth: 760 }}>
              {t('home.hero.title')}
            </Typography>
            <Typography variant="body1" sx={{ maxWidth: 610, color: { xs: 'text.secondary', md: 'inherit' }, opacity: { md: 0.94 }, fontSize: { xs: '1rem', md: '1.25rem' } }}>
              {t('home.hero.subtitle')}
            </Typography>
            <Button component={RouterLink} to="/complexes" variant="contained" size="large" sx={{ minHeight: 48, px: 3, fontWeight: 800, width: { xs: '100%', sm: 'auto' }, ...responsiveCtaSx }}>
              {t('home.hero.browse')}
            </Button>
            <Stack direction="row" spacing={2} sx={{ display: { xs: 'none', md: 'flex' } }}>
              <Button component={RouterLink} to="/login?intent=user" color="inherit">{t('home.hero.play')}</Button>
              <Button component={RouterLink} to="/login?intent=complex" color="inherit">{t('home.hero.manage')}</Button>
            </Stack>
          </Stack>
        </Container>
      </Box>

      <Suspense fallback={<Box component="section" sx={{ py: { xs: 5, md: 9 } }}><Container maxWidth="lg"><Skeleton variant="text" width={220} height={40} /><Grid container spacing={2}>{[0, 1, 2].map((index) => <Grid key={index} size={{ xs: 12, sm: 6, md: 4 }}><Skeleton variant="rounded" height={210} /></Grid>)}</Grid></Container></Box>}>
        <HomeFeaturedSection />
      </Suspense>

      <Container id="how-it-works" component="section" aria-labelledby="steps-title" maxWidth="lg" sx={{ py: { xs: 5, md: 9 } }}>
        <Stack spacing={1} sx={{ mb: { xs: 3, md: 4 } }}>
          <Typography id="steps-title" component="h2" variant="h4" sx={{ fontWeight: 800 }}>{t('home.steps.title')}</Typography>
          <Typography color="text.secondary">{t('home.steps.subtitle')}</Typography>
        </Stack>
        <Grid container spacing={{ xs: 1.5, md: 2.5 }}>
          {steps.map((step, index) => (
            <Grid key={step.title} size={{ xs: 12, md: 4 }}>
              <Stack direction={{ xs: 'row', md: 'column' }} spacing={2} alignItems={{ xs: 'center', md: 'flex-start' }} sx={{ p: { xs: 1.25, md: 2 } }}>
                <Box aria-hidden="true" sx={{ flex: '0 0 auto', width: 36, height: 36, display: 'grid', placeItems: 'center', borderRadius: '50%', bgcolor: 'primary.main', color: 'common.white', fontWeight: 800 }}>{stepIcons[index]}</Box>
                <Box><Typography component="h3" variant="subtitle1" sx={{ fontWeight: 800 }}>{step.title}</Typography><Typography variant="body2" color="text.secondary">{step.description}</Typography></Box>
              </Stack>
            </Grid>
          ))}
        </Grid>
      </Container>

      <Box component="section" aria-labelledby="owner-title" sx={{ bgcolor: 'background.paper', borderTop: 1, borderColor: 'divider', py: { xs: 5, md: 8 } }}>
        <Container maxWidth="md">
          <Card variant="outlined" sx={{ borderRadius: 3, boxShadow: 'none' }}>
            <CardContent sx={{ p: { xs: 2.5, sm: 4 } }}>
              <Stack spacing={2} alignItems={{ xs: 'flex-start', sm: 'center' }} textAlign={{ sm: 'center' }}>
                <Typography id="owner-title" component="h2" variant="h5" sx={{ fontWeight: 800 }}>{t('home.audiences.ownersTitle')}</Typography>
                <Typography color="text.secondary">{t('home.audiences.ownersDescription')}</Typography>
                <Button component={RouterLink} to="/login?intent=complex" variant="outlined" sx={{ minHeight: 44, ...responsiveCtaSx }}>{t('home.audiences.ownersAction')}</Button>
              </Stack>
            </CardContent>
          </Card>
        </Container>
      </Box>
    </Box>
  );
}
