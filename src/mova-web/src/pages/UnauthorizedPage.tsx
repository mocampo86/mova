import { Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import Seo from '../components/Seo';

export default function UnauthorizedPage() {
  const { t } = useTranslation();

  return (
    <>
      <Seo
        title={`${t('common.appName')} | ${t('unauthorized.title')}`}
        description={t('seo.unauthorizedDescription')}
      />
      <Typography variant="h4">
        {t('unauthorized.title')}
      </Typography>
    </>
  );
}
