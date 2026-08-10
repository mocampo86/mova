import { Typography } from '@mui/material';
import { useTranslation } from 'react-i18next';

export default function UserHomePage() {
  const { t } = useTranslation();

  return (
    <Typography variant="body1">
      {t('userHome.message')}
    </Typography>
  );
}
