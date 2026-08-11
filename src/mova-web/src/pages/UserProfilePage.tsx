import { Link as RouterLink } from 'react-router-dom';
import { Alert, Button, Card, CardContent, Container, Skeleton, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { useUserDashboard } from '../features/users/useUserDashboard';

export default function UserProfilePage() {
  const { t } = useTranslation();
  const { data, isLoading, isError } = useUserDashboard();

  return (
    <Container maxWidth="sm" sx={{ py: 4 }}>
      <Typography component="h1" variant="h4" sx={{ fontWeight: 800, mb: 3 }}>
        {t('dashboard.profileTitle')}
      </Typography>

      {isError && <Alert severity="error">{t('dashboard.profileError')}</Alert>}

      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            {isLoading && (
              <>
                <Skeleton variant="text" width="80%" />
                <Skeleton variant="text" width="60%" />
                <Skeleton variant="text" width="50%" />
              </>
            )}
            {!isLoading && data && (
              <>
                <Typography>
                  <strong>{t('common.name')}:</strong> {data.user.fullName}
                </Typography>
                <Typography>
                  <strong>{t('common.email')}:</strong> {data.user.email}
                </Typography>
                <Typography>
                  <strong>{t('common.phone')}:</strong> {data.user.phoneNumber ?? t('common.emptyValue')}
                </Typography>
                <Typography>
                  <strong>{t('dashboard.phoneVerified')}:</strong>{' '}
                  {data.user.phoneVerified ? t('common.yes') : t('common.no')}
                </Typography>
                <Button component={RouterLink} to="/complete-profile" variant="contained" sx={{ mt: 2 }}>
                  {t('dashboard.updatePhone')}
                </Button>
              </>
            )}
          </Stack>
        </CardContent>
      </Card>
    </Container>
  );
}
