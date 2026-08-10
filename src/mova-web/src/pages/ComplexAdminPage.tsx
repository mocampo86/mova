import { Link as RouterLink, useParams } from 'react-router-dom';
import {
  Alert,
  Box,
  Card,
  CardActionArea,
  CardContent,
  Chip,
  Container,
  Grid,
  Skeleton,
  Stack,
  Typography
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useComplexDashboard } from '../features/complexes/complexApi';

function formatLastUpdated(t: (key: string, options?: Record<string, unknown>) => string, isoString?: string | null) {
  if (!isoString) return t('common.notUpdatedYet');
  const date = new Date(isoString);
  return t('common.lastUpdated', { date: `${date.toLocaleDateString()} ${date.toLocaleTimeString()}` });
}

function statusColor(status: string): 'success' | 'default' | 'warning' {
  if (status === 'Active') return 'success';
  if (status === 'Inactive') return 'warning';
  return 'default';
}

interface DashboardCardProps {
  title: string;
  value: number;
  to: string;
  isLoading?: boolean;
}

function DashboardCard({ title, value, to, isLoading }: DashboardCardProps) {
  return (
    <Card variant="outlined" sx={{ height: '100%' }}>
      <CardActionArea component={RouterLink} to={to} sx={{ height: '100%' }}>
        <CardContent>
          {isLoading ? (
            <>
              <Skeleton variant="text" width="60%" />
              <Skeleton variant="rectangular" width="40%" height={40} />
            </>
          ) : (
            <>
              <Typography color="text.secondary" variant="body2">
                {title}
              </Typography>
              <Typography component="p" variant="h4" sx={{ fontWeight: 700, mt: 1 }}>
                {value}
              </Typography>
            </>
          )}
        </CardContent>
      </CardActionArea>
    </Card>
  );
}

export default function ComplexAdminPage() {
  const { t } = useTranslation();
  const { complexId = '' } = useParams();
  const { data, isLoading, isError } = useComplexDashboard(complexId);

  if (isError) {
    return (
      <Container sx={{ py: 4 }}>
        <Alert severity="error">{t('admin.dashboard.error')}</Alert>
      </Container>
    );
  }

  const complex = data?.complex;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={4}>
        <Box>
          {isLoading || !complex ? (
            <>
              <Skeleton variant="text" width="50%" height={48} />
              <Skeleton variant="text" width="30%" />
            </>
          ) : (
            <>
              <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap">
                <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
                  {complex.name}
                </Typography>
                <Chip label={complex.status} color={statusColor(complex.status)} size="small" />
              </Stack>
              <Typography color="text.secondary" sx={{ mt: 1 }}>
                {formatLastUpdated(t, complex.lastUpdatedAt)}
              </Typography>
            </>
          )}
        </Box>

        <Grid container spacing={3}>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <DashboardCard
              title={t('admin.dashboard.activeCourts')}
              value={data?.courts.active ?? 0}
              to={`/admin/complex/${complexId}/courts`}
              isLoading={isLoading}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <DashboardCard
              title={t('admin.dashboard.inactiveCourts')}
              value={data?.courts.inactive ?? 0}
              to={`/admin/complex/${complexId}/courts`}
              isLoading={isLoading}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <DashboardCard
              title={t('admin.dashboard.confirmedToday')}
              value={data?.reservationsToday.confirmed ?? 0}
              to={`/admin/complex/${complexId}/reservations`}
              isLoading={isLoading}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <DashboardCard
              title={t('admin.dashboard.completedToday')}
              value={data?.reservationsToday.completed ?? 0}
              to={`/admin/complex/${complexId}/reservations`}
              isLoading={isLoading}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <DashboardCard
              title={t('admin.dashboard.cancelledToday')}
              value={data?.reservationsToday.cancelled ?? 0}
              to={`/admin/complex/${complexId}/reservations`}
              isLoading={isLoading}
            />
          </Grid>
          <Grid size={{ xs: 12, sm: 6, md: 3 }}>
            <DashboardCard
              title={t('admin.dashboard.blockedUsers')}
              value={data?.blockedUsers ?? 0}
              to={`/admin/complex/${complexId}/users`}
              isLoading={isLoading}
            />
          </Grid>
        </Grid>
      </Stack>
    </Container>
  );
}
