import { Container, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';

export default function ComplexAdminPlaceholderPage() {
  const { t } = useTranslation();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h5" component="h2" gutterBottom>
        {t('admin.placeholder.title')}
      </Typography>
      <Typography color="text.secondary">
        {t('admin.placeholder.subtitle')}
      </Typography>
    </Container>
  );
}
