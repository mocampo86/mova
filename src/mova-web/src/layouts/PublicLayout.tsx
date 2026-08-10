import { Outlet } from 'react-router-dom';
import { Container, Stack, Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import LanguageSelector from '../components/LanguageSelector';

export default function PublicLayout() {
  const { t } = useTranslation();

  return (
    <Container maxWidth="lg">
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        sx={{ mt: 2, mb: 1 }}
      >
        <Typography variant="h4" component="header">
          {t('common.appName')}
        </Typography>
        <LanguageSelector />
      </Stack>
      <main>
        <Outlet />
      </main>
    </Container>
  );
}
