import { Link as RouterLink } from 'react-router-dom';
import { Box, Button, Card, CardContent, Container, Grid, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';

export default function HomePage() {
  const { t } = useTranslation();

  const benefits = [
    {
      title: t('home.benefits.discoverTitle'),
      description: t('home.benefits.discoverDescription')
    },
    {
      title: t('home.benefits.chooseTitle'),
      description: t('home.benefits.chooseDescription')
    },
    {
      title: t('home.benefits.playTitle'),
      description: t('home.benefits.playDescription')
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

      <Container id="how-it-works" component="section" aria-labelledby="benefits-title" maxWidth="lg" sx={{ py: { xs: 7, md: 10 } }}>
        <Stack spacing={2} alignItems="center" textAlign="center" sx={{ mb: 5 }}>
          <Typography id="benefits-title" component="h2" variant="h4" sx={{ fontWeight: 700 }}>
            {t('home.benefits.title')}
          </Typography>
          <Typography color="text.secondary" sx={{ maxWidth: 620 }}>
            {t('home.benefits.subtitle')}
          </Typography>
        </Stack>
        <Grid container spacing={3}>
          {benefits.map((benefit) => (
            <Grid key={benefit.title} size={{ xs: 12, md: 4 }}>
              <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
                <CardContent sx={{ p: 3 }}>
                  <Typography component="h3" variant="h6" sx={{ fontWeight: 700, mb: 1 }}>
                    {benefit.title}
                  </Typography>
                  <Typography color="text.secondary">{benefit.description}</Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>

      <Box component="section" aria-labelledby="cta-title" sx={{ bgcolor: 'grey.100', py: 7, px: 2, textAlign: 'center' }}>
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
