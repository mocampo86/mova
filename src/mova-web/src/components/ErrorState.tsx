import Alert from '@mui/material/Alert';
import AlertTitle from '@mui/material/AlertTitle';
import { useTranslation } from 'react-i18next';

export interface ErrorStateProps {
  title?: string;
  message?: string;
}

export default function ErrorState({
  title,
  message
}: ErrorStateProps) {
  const { t } = useTranslation();

  return (
    <Alert severity="error" sx={{ mt: 2 }}>
      <AlertTitle>{title ?? t('common.error.title')}</AlertTitle>
      {message ?? t('common.error.message')}
    </Alert>
  );
}
