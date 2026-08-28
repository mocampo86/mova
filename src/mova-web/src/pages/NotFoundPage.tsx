import { Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';
import Seo from '../components/Seo';

export default function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <>
      <Seo
        title={`${t('common.appName')} | ${t('notFound.title')}`}
        description={t('seo.notFoundDescription')}
      />
      <Typography variant="h5">
        {t('notFound.title')}
      </Typography>
    </>
  );
}
