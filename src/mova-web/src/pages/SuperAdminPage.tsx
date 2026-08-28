import { Button, Container, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import { Link as RouterLink } from 'react-router-dom';

export default function SuperAdminPage() {
  const { t } = useTranslation();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Stack spacing={2}>
        <Typography component="h1" variant="h4" sx={{ fontWeight: 800 }}>
          {t('superAdmin.title')}
        </Typography>
        <Button component={RouterLink} to="/admin/super/audit-logs" variant="contained">
          {t('superAdmin.viewAuditLog')}
        </Button>
      </Stack>
    </Container>
  );
}
