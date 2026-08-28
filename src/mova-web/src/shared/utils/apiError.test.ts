import { describe, expect, it } from 'vitest';
import i18n from '../../i18n';
import { ApiError, getApiErrorMessage, getApiErrorTranslationKey } from './apiError';

describe('apiError', () => {
  const t = i18n.getFixedT('en');

  it('maps known API error codes to translation keys', () => {
    expect(getApiErrorTranslationKey('NOT_FOUND')).toBe('common.error.notFound');
    expect(getApiErrorTranslationKey('RESERVATION_CONFLICT')).toBe('common.error.conflict');
    expect(getApiErrorTranslationKey('UNKNOWN_CODE')).toBeUndefined();
  });

  it('returns an empty string for a missing error', () => {
    expect(getApiErrorMessage(null, t)).toBe('');
    expect(getApiErrorMessage(undefined, t)).toBe('');
  });

  it('returns the message for a generic Error', () => {
    expect(getApiErrorMessage(new Error('Load failed'), t)).toBe('Load failed');
  });

  it('translates a known ApiError code', () => {
    const error = new ApiError(404, 'Court not found', 'NOT_FOUND');
    expect(getApiErrorMessage(error, t)).toBe('The requested resource was not found.');
  });

  it('falls back to the ApiError message when the code is not mapped', () => {
    const error = new ApiError(500, 'Internal failure', 'UNKNOWN');
    expect(getApiErrorMessage(error, t)).toBe('Internal failure');
  });

  it('falls back to the generic message for non-Error values', () => {
    expect(getApiErrorMessage({ some: 'thing' }, t)).toBe(
      'An unexpected error occurred. Please try again later.'
    );
  });
});
