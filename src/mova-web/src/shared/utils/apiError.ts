import type { TFunction } from 'i18next';

export interface StructuredApiErrorBody {
  error?: {
    code?: string;
    message?: string;
    traceId?: string;
    details?: Record<string, unknown>;
  };
  message?: string;
}

export class ApiError extends Error {
  public status: number;
  public code?: string;
  public traceId?: string;
  public details?: Record<string, unknown>;

  constructor(
    status: number,
    message: string,
    code?: string,
    traceId?: string,
    details?: Record<string, unknown>
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.code = code;
    this.traceId = traceId;
    this.details = details;
  }
}

const ERROR_CODE_KEY_MAP: Record<string, string> = {
  VALIDATION_ERROR: 'common.error.validation',
  NOT_FOUND: 'common.error.notFound',
  UNAUTHORIZED: 'common.error.unauthorized',
  FORBIDDEN: 'common.error.forbidden',
  USER_BLOCKED: 'common.error.blockedUser',
  RESERVATION_CONFLICT: 'common.error.conflict',
  RECURRING_RESERVATIONS_DISABLED: 'common.error.recurringDisabled',
  CONCURRENCY_ERROR: 'common.error.concurrency',
  RATE_LIMIT_EXCEEDED: 'common.error.rateLimit'
};

export function getApiErrorTranslationKey(code?: string): string | undefined {
  if (!code) return undefined;
  return ERROR_CODE_KEY_MAP[code];
}

export function getApiErrorMessage(error: unknown, t: TFunction): string {
  if (error == null) return '';

  if (error instanceof ApiError) {
    const key = getApiErrorTranslationKey(error.code);
    if (key) return t(key);
    return error.message || t('common.error.message');
  }

  if (error instanceof Error) {
    return error.message;
  }

  if (typeof error === 'string') {
    return error;
  }

  return t('common.error.message');
}
