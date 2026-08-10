import { Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';

export default function NotFoundPage() {
  const { t } = useTranslation();

  return (
    <Typography variant="h5">
      {t('notFound.title')}
    </Typography>
  );
}
