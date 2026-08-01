export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

const SENSITIVE_KEYS = [
  'password',
  'token',
  'secret',
  'apikey',
  'api_key',
  'authorization',
  'auth',
  'jwt',
  'bearer',
  'cookie',
  'email',
  'phone',
  'phonenumber',
  'mobile',
  'fullname',
  'creditcard',
  'ssn',
  'stack',
  'stacktrace'
];

function isSensitiveKey(key: string): boolean {
  const normalized = key.toLowerCase();
  return SENSITIVE_KEYS.some((sensitive) => normalized.includes(sensitive));
}

export function redact(value: unknown): unknown {
  if (value === null || value === undefined) {
    return value;
  }

  if (typeof value !== 'object') {
    return value;
  }

  if (value instanceof Error) {
    const cause = (value as { cause?: unknown }).cause;
    return {
      name: value.name,
      message: value.message,
      stack: '[REDACTED]',
      cause: cause !== undefined ? redact(cause) : undefined
    };
  }

  if (Array.isArray(value)) {
    return value.map(redact);
  }

  const record = value as Record<string, unknown>;
  const result: Record<string, unknown> = {};

  for (const [key, childValue] of Object.entries(record)) {
    result[key] = isSensitiveKey(key) ? '[REDACTED]' : redact(childValue);
  }

  return result;
}

function createLogEntry(level: LogLevel, message: string, context?: Record<string, unknown>) {
  return {
    timestamp: new Date().toISOString(),
    level: level.toUpperCase(),
    message,
    context: context ? redact(context) : undefined
  };
}

export const logger = {
  debug: (message: string, context?: Record<string, unknown>) => {
    console.debug(createLogEntry('debug', message, context));
  },
  info: (message: string, context?: Record<string, unknown>) => {
    console.info(createLogEntry('info', message, context));
  },
  warn: (message: string, context?: Record<string, unknown>) => {
    console.warn(createLogEntry('warn', message, context));
  },
  error: (message: string, context?: Record<string, unknown>) => {
    console.error(createLogEntry('error', message, context));
  }
};

export default logger;
