import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthContext } from '../auth/AuthContext';
import type { AuthState } from '../auth/authTypes';
import { useCourts } from './courtApi';
import type { CourtListFilters } from './courtTypes';

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

describe('useCourts', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          items: [],
          page: 1,
          pageSize: 10,
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
    const filters: CourtListFilters = {
      page: 0,
      pageSize: 10,
      status: 'All',
      sportId: '',
      search: ''
    };

    const { result } = renderHook(() => useCourts('complex-1', filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/courts?page=1'),
      expect.any(Object)
    );
  });
});
