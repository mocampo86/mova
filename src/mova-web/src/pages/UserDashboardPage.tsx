import { Link as RouterLink } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Container,
  Grid,
  Skeleton,
  Stack,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useUserDashboard } from '../features/users/useUserDashboard';

function formatLocalDateTime(isoString: string): string {
  const date = new Date(isoString);
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
}

export default function UserDashboardPage() {
  const { t } = useTranslation();
  const { data, isLoading, isError } = useUserDashboard();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={4}>
        {isError && <Alert severity="error">{t('dashboard.errorMessage')}</Alert>}

        <Box>
          <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
            {isLoading || !data ? (
              <Skeleton variant="text" width={260} />
            ) : (
              t('dashboard.welcome', { name: data.user.fullName })
            )}
          </Typography>
          {!isLoading && data && (
            <Typography color="text.secondary">{data.user.email}</Typography>
          )}
        </Box>

        {data?.activeBlocks && data.activeBlocks.length > 0 && (
          <Alert severity="warning" variant="outlined">
            <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
              {t('dashboard.activeBlocksTitle')}
            </Typography>
            <Stack component="ul" spacing={1} sx={{ pl: 2, m: 0 }}>
              {data.activeBlocks.map((block) => (
                <Typography component="li" key={block.id}>
                  {t('dashboard.activeBlockMessage', {
                    complex: block.complexName,
                    reason: block.reason ?? t('dashboard.noReason')
                  })}
                </Typography>
              ))}
            </Stack>
          </Alert>
        )}

        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardContent>
                <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
                  {t('dashboard.upcomingTitle')}
                </Typography>

                {isLoading && (
                  <Stack spacing={1} sx={{ mt: 2 }}>
                    <Skeleton variant="text" />
                    <Skeleton variant="text" />
                    <Skeleton variant="text" />
                  </Stack>
                )}

                {!isLoading && !isError && data && (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    {data.upcomingReservations.items.length === 0 && (
                      <Typography color="text.secondary">{t('dashboard.noUpcomingReservations')}</Typography>
                    )}
                    {data.upcomingReservations.items.slice(0, 5).map((reservation) => (
                      <Box key={reservation.id}>
                        <Typography sx={{ fontWeight: 600 }}>{reservation.courtName}</Typography>
                        <Typography variant="body2" color="text.secondary">
                          {formatLocalDateTime(reservation.startAt)} - {formatLocalDateTime(reservation.endAt)}
                        </Typography>
                      </Box>
                    ))}
                  </Stack>
                )}
              </CardContent>
              <CardActions>
                <Button component={RouterLink} to="/user/reservations" size="small">
                  {t('dashboard.viewAllReservations')}
                </Button>
                <Button component={RouterLink} to="/complexes" size="small" variant="contained">
                  {t('dashboard.newReservation')}
                </Button>
              </CardActions>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, md: 6 }}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardContent>
                <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
                  {t('dashboard.historyTitle')}
                </Typography>

                {isLoading && (
                  <Stack spacing={1} sx={{ mt: 2 }}>
                    <Skeleton variant="text" />
                    <Skeleton variant="text" />
                    <Skeleton variant="text" />
                  </Stack>
                )}

                {!isLoading && !isError && data && (
                  <Stack spacing={2} sx={{ mt: 2 }}>
                    <Typography>
                      {t('dashboard.historyCount', { count: data.historySummary.totalItems })}
                    </Typography>
                    {data.historySummary.recentReservations.length === 0 && (
                      <Typography color="text.secondary">{t('dashboard.noHistory')}</Typography>
                    )}
                    {data.historySummary.recentReservations.map((reservation) => (
                      <Box key={reservation.id}>
                        <Typography sx={{ fontWeight: 600 }}>{reservation.courtName}</Typography>
                        <Typography variant="body2" color="text.secondary">
                          {formatLocalDateTime(reservation.startAt)} - {formatLocalDateTime(reservation.endAt)}
                        </Typography>
                      </Box>
                    ))}
                  </Stack>
                )}
              </CardContent>
              <CardActions>
                <Button component={RouterLink} to="/user/history" size="small">
                  {t('dashboard.viewAllHistory')}
                </Button>
              </CardActions>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardContent>
                <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
                  {t('dashboard.exploreComplexesTitle')}
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 1 }}>
                  {t('dashboard.exploreComplexesDescription')}
                </Typography>
              </CardContent>
              <CardActions>
                <Button component={RouterLink} to="/complexes" size="small" variant="contained">
                  {t('dashboard.browseComplexes')}
                </Button>
              </CardActions>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardContent>
                <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
                  {t('dashboard.recurringTitle')}
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 1 }}>
                  {t('dashboard.recurringDescription')}
                </Typography>
              </CardContent>
              <CardActions>
                <Button component={RouterLink} to="/complexes" size="small" variant="contained">
                  {t('dashboard.newRecurringReservation')}
                </Button>
              </CardActions>
            </Card>
          </Grid>

          <Grid size={{ xs: 12, sm: 6, md: 4 }}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardContent>
                <Typography component="h2" variant="h6" sx={{ fontWeight: 700 }}>
                  {t('dashboard.profileTitle')}
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 1 }}>
                  {t('dashboard.profileDescription')}
                </Typography>
              </CardContent>
              <CardActions>
                <Button component={RouterLink} to="/user/profile" size="small" variant="contained">
                  {t('dashboard.viewProfile')}
                </Button>
              </CardActions>
            </Card>
          </Grid>
        </Grid>
      </Stack>
    </Container>
  );
}
