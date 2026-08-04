import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from './apiClient';

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
      'http://localhost:5000/api/v1/test',
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
});
