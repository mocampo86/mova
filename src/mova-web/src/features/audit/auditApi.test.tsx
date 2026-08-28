import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthContext } from '../auth/AuthContext';
import type { AuthState } from '../auth/authTypes';
import { useAuditLogs } from './auditApi';
import type { AuditLogFilters } from './auditTypes';

const mockAuthState: AuthState = {
  accessToken: 'test-token',
  user: null,
  isAuthenticated: false,
  requiresProfileCompletion: false,
  login: vi.fn(),
  logout: vi.fn(),
  completeProfile: vi.fn()
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false }
    }
  });

  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        <AuthContext.Provider value={mockAuthState}>{children}</AuthContext.Provider>
      </QueryClientProvider>
    );
  };
}

describe('useAuditLogs', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          items: [],
          page: 1,
          pageSize: 25,
          totalItems: 0,
          totalPages: 0
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it('sends a 1-based page number to the API', async () => {
    const filters: AuditLogFilters = {
      page: 0,
      pageSize: 25,
      action: '',
      entityType: '',
      entityId: '',
      sportsComplexId: '',
      userId: '',
      from: '',
      to: ''
    };

    const { result } = renderHook(() => useAuditLogs(filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/admin/audit-logs?page=1'),
      expect.any(Object)
    );
  });

  it('includes filters in the query string', async () => {
    const filters: AuditLogFilters = {
      page: 1,
      pageSize: 10,
      action: 'Court.Create',
      entityType: 'Court',
      entityId: 'court-1',
      sportsComplexId: 'complex-1',
      userId: 'user-1',
      from: '2026-08-01T00:00',
      to: '2026-08-31T23:59'
    };

    const { result } = renderHook(() => useAuditLogs(filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    const calledUrl = fetchSpy.mock.calls[0][0] as string;
    expect(calledUrl).toContain('/api/v1/admin/audit-logs?');
    expect(calledUrl).toContain('page=2');
    expect(calledUrl).toContain('pageSize=10');
    expect(calledUrl).toContain('action=Court.Create');
    expect(calledUrl).toContain('entityType=Court');
    expect(calledUrl).toContain('entityId=court-1');
    expect(calledUrl).toContain('sportsComplexId=complex-1');
    expect(calledUrl).toContain('userId=user-1');
    expect(calledUrl).toContain('from=2026-08-01T00%3A00');
    expect(calledUrl).toContain('to=2026-08-31T23%3A59');
  });
});
