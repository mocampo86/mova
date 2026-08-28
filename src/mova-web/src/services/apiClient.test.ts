import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ApiError } from '../shared/utils/apiError';
import { apiClient, API_BASE_URL } from './apiClient';

describe('apiClient', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ id: '1' }), {
        status: 200,
        headers: { 'Content-Type': 'application/json' }
      })
    );
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it('sets Content-Type and sends the request body', async () => {
    await apiClient('/api/v1/test', { method: 'POST', body: JSON.stringify({ value: 1 }) });

    expect(fetchSpy).toHaveBeenCalledWith(
      `${API_BASE_URL}/api/v1/test`,
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ value: 1 }),
        headers: expect.any(Headers)
      })
    );

    const callArgs = fetchSpy.mock.calls[0] as [string, RequestInit];
    const headers = callArgs[1].headers as Headers;
    expect(headers.get('Content-Type')).toBe('application/json');
  });

  it('attaches the Authorization header when an access token is provided', async () => {
    await apiClient('/api/v1/test', { method: 'GET' }, 'test-access-token');

    const callArgs = fetchSpy.mock.calls[0] as [string, RequestInit];
    const headers = callArgs[1].headers as Headers;
    expect(headers.get('Authorization')).toBe('Bearer test-access-token');
  });

  it('does not attach an Authorization header when no access token is provided', async () => {
    await apiClient('/api/v1/test', { method: 'GET' });

    const callArgs = fetchSpy.mock.calls[0] as [string, RequestInit];
    const headers = callArgs[1].headers as Headers;
    expect(headers.get('Authorization')).toBeNull();
  });

  it('returns the parsed JSON response', async () => {
    const result = await apiClient<{ id: string }>('/api/v1/test', { method: 'GET' });

    expect(result).toEqual({ id: '1' });
  });

  it('throws an error when the response is not ok', async () => {
    fetchSpy.mockResolvedValueOnce(new Response('Bad request', { status: 400 }));

    await expect(apiClient('/api/v1/test', { method: 'GET' })).rejects.toThrow('Bad request');
  });

  it('throws an ApiError with code and trace id for a structured error response', async () => {
    const body = JSON.stringify({
      error: {
        code: 'VALIDATION_ERROR',
        message: 'The request is invalid.',
        traceId: '00-123'
      }
    });

    fetchSpy.mockResolvedValueOnce(
      new Response(body, { status: 400, headers: { 'Content-Type': 'application/json' } })
    );

    try {
      await apiClient('/api/v1/test', { method: 'GET' });
      expect.fail('apiClient should have thrown');
    } catch (error) {
      expect(error).toBeInstanceOf(ApiError);
      const apiError = error as ApiError;
      expect(apiError.status).toBe(400);
      expect(apiError.code).toBe('VALIDATION_ERROR');
      expect(apiError.message).toBe('The request is invalid.');
      expect(apiError.traceId).toBe('00-123');
    }
  });
});
