import { ApiError } from '../shared/utils/apiError';

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5098';

export async function apiClient<T>(
  path: string,
  options: RequestInit = {},
  accessToken?: string
): Promise<T> {
  const headers = new Headers(options.headers ?? undefined);

  if (!headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers
  });

  if (!response.ok) {
    const bodyText = await response.text();
    let message = bodyText || `Request failed with status ${response.status}`;
    let code: string | undefined;
    let traceId: string | undefined;
    let details: Record<string, unknown> | undefined;

    if (bodyText) {
      try {
        const body = JSON.parse(bodyText) as {
          error?: { code?: string; message?: string; traceId?: string; details?: Record<string, unknown> };
          message?: string;
        };

        if (body.error) {
          code = body.error.code;
          message = body.error.message || message;
          traceId = body.error.traceId;
          details = body.error.details;
        } else if (body.message) {
          message = body.message;
        }
      } catch {
        // Response body is not JSON; use the raw text as the message.
      }
    }

    throw new ApiError(response.status, message, code, traceId, details);
  }

  return response.json() as Promise<T>;
}
