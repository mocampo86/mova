import { Link as RouterLink } from 'react-router-dom';
import { Box, Button, Card, CardContent, Container, Grid, Stack, Typography } from '@mui/material';

export default function HomePage() {
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
              Sport, simplified
            </Typography>
            <Typography id="hero-title" component="h1" variant="h2" sx={{ fontWeight: 800, fontSize: { xs: '2.5rem', md: '4rem' } }}>
              Find your next game.
            </Typography>
            <Typography variant="h6" sx={{ maxWidth: 650, fontWeight: 400, opacity: 0.92 }}>
              Discover nearby sports complexes, check court availability, and reserve the time that works for you.
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Button component={RouterLink} to="/login" variant="contained" size="large" color="secondary">
                Get started
              </Button>
              <Button href="#how-it-works" variant="outlined" size="large" sx={{ color: 'common.white', borderColor: 'rgba(255,255,255,.7)' }}>
                Learn more
              </Button>
            </Stack>
          </Stack>
        </Container>
      </Box>

      <Container id="how-it-works" component="section" aria-labelledby="benefits-title" maxWidth="lg" sx={{ py: { xs: 7, md: 10 } }}>
        <Stack spacing={2} alignItems="center" textAlign="center" sx={{ mb: 5 }}>
          <Typography id="benefits-title" component="h2" variant="h4" sx={{ fontWeight: 700 }}>
            Everything you need to play
          </Typography>
          <Typography color="text.secondary" sx={{ maxWidth: 620 }}>
            Mova makes finding and booking sports facilities simple, so you can spend less time coordinating and more time on the court.
          </Typography>
        </Stack>
        <Grid container spacing={3}>
          {[
            ['Discover', 'Explore active sports complexes and find a facility that fits your game.'],
            ['Choose', 'See the courts and sports available at each complex before you commit.'],
            ['Play', 'Pick a convenient time and keep your reservations organized in one place.']
          ].map(([title, description]) => (
            <Grid key={title} size={{ xs: 12, md: 4 }}>
              <Card variant="outlined" sx={{ height: '100%', borderRadius: 3 }}>
                <CardContent sx={{ p: 3 }}>
                  <Typography component="h3" variant="h6" sx={{ fontWeight: 700, mb: 1 }}>
                    {title}
                  </Typography>
                  <Typography color="text.secondary">{description}</Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>

      <Box component="section" aria-labelledby="cta-title" sx={{ bgcolor: 'grey.100', py: 7, px: 2, textAlign: 'center' }}>
        <Stack spacing={2} alignItems="center">
          <Typography id="cta-title" component="h2" variant="h5" sx={{ fontWeight: 700 }}>
            Ready to get moving?
          </Typography>
          <Typography color="text.secondary">Join Mova and make your next match easier to plan.</Typography>
          <Button component={RouterLink} to="/login" variant="contained" size="large">
            Sign in to Mova
          </Button>
        </Stack>
      </Box>
    </Box>
  );
}
