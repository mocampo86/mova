import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { getApiErrorMessage } from '../shared/utils/apiError';

interface ApiErrorMessageProps {
  error: unknown;
}

export default function ApiErrorMessage({ error }: ApiErrorMessageProps): ReactNode {
  const { t } = useTranslation();

  if (error == null) return null;

  return <>{getApiErrorMessage(error, t)}</>;
}
