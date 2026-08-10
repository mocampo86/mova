import { Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';

export default function SuperAdminPage() {
  const { t } = useTranslation();

  return <Typography variant="h4">{t('superAdmin.title')}</Typography>;
}
