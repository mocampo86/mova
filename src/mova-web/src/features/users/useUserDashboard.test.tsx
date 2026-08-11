import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthContext } from '../auth/AuthContext';
import type { AuthState } from '../auth/authTypes';
import { useUserDashboard } from './useUserDashboard';

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

describe('useUserDashboard', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          user: {
            id: 'user-1',
            email: 'user@example.com',
            fullName: 'Test User',
            phoneNumber: '+1234567890',
            phoneVerified: false
          },
          upcomingReservations: {
            items: [],
            page: 1,
            pageSize: 5,
            totalItems: 0,
            totalPages: 0
          },
          historySummary: {
            totalItems: 0,
            recentReservations: []
          },
          activeBlocks: []
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

  it('fetches the user dashboard summary', async () => {
    const { result } = renderHook(() => useUserDashboard(), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.user.fullName).toBe('Test User');
    expect(result.current.data?.upcomingReservations.totalItems).toBe(0);
    expect(result.current.data?.historySummary.totalItems).toBe(0);
    expect(result.current.data?.activeBlocks).toHaveLength(0);

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/users/me/dashboard?'),
      expect.any(Object)
    );

    const url = fetchSpy.mock.calls[0][0] as string;
    expect(url).toContain('upcomingPageSize=5');
    expect(url).toContain('historyPageSize=3');
  });
});
