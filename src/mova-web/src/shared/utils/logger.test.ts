import { describe, it, expect, vi } from 'vitest';
import { logger, redact } from './logger';

describe('logger', () => {
  it('logs a structured entry with timestamp, level and message', () => {
    const spy = vi.spyOn(console, 'info').mockReturnValue(undefined);

    logger.info('User action', { action: 'view_court' });

    expect(spy).toHaveBeenCalledOnce();
    const entry = spy.mock.calls[0][0] as Record<string, unknown>;
    expect(entry.level).toBe('INFO');
    expect(entry.message).toBe('User action');
    expect(entry.timestamp).toBeDefined();
    expect((entry.context as Record<string, unknown>).action).toBe('view_court');

    spy.mockRestore();
  });

  it('redacts sensitive fields before logging', () => {
    const spy = vi.spyOn(console, 'error').mockReturnValue(undefined);

    logger.error('Login failed', {
      email: 'user@example.com',
      password: 'super-secret',
      token: 'Bearer abc123'
    });

    const entry = spy.mock.calls[0][0] as Record<string, unknown>;
    const context = entry.context as Record<string, unknown>;

    expect(context.email).toBe('[REDACTED]');
    expect(context.password).toBe('[REDACTED]');
    expect(context.token).toBe('[REDACTED]');

    spy.mockRestore();
  });
});

describe('redact', () => {
  it('redacts stack trace in error objects', () => {
    const error = new Error('Something failed');
    const redacted = redact({ error }) as Record<string, unknown>;
    const redactedError = redacted.error as Record<string, unknown>;

    expect(redactedError.message).toBe('Something failed');
    expect(redactedError.stack).toBe('[REDACTED]');
  });

  it('does not alter non-sensitive values', () => {
    const redacted = redact({ courtName: 'Court A', capacity: 10 });

    expect(redacted).toEqual({ courtName: 'Court A', capacity: 10 });
  });
});
