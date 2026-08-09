import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthContext } from '../auth/AuthContext';
import type { AuthState } from '../auth/authTypes';
import { updateCourt, useCourt, useCourts, useUpdateCourt } from './courtApi';
import type { CourtListFilters, UpdateCourtRequest } from './courtTypes';

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

describe('useCourt', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'court-1',
          sportsComplexId: 'complex-1',
          name: 'Court One',
          description: 'Description',
          surfaceType: 'Synthetic',
          indoor: false,
          status: 'Active',
          sportIds: []
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

  it('fetches the court from the admin endpoint', async () => {
    const { result } = renderHook(() => useCourt('complex-1', 'court-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/courts/court-1'),
      expect.any(Object)
    );
  });
});

describe('useUpdateCourt', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'court-1',
          sportsComplexId: 'complex-1',
          name: 'Updated Court',
          description: 'Updated',
          surfaceType: 'Grass',
          indoor: true,
          status: 'Active',
          sportIds: []
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

  it('sends a PUT request to the court endpoint', async () => {
    const request: UpdateCourtRequest = {
      name: 'Updated Court',
      description: 'Updated',
      surfaceType: 'Grass',
      indoor: true,
      sportIds: []
    };

    const { result } = renderHook(() => useUpdateCourt('complex-1', 'court-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync(request);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/courts/court-1'),
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify(request)
      })
    );
  });
});

describe('updateCourt', () => {
  it('sends a PUT request with the given payload', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'court-1',
          sportsComplexId: 'complex-1',
          name: 'Updated Court',
          description: 'Updated',
          surfaceType: 'Grass',
          indoor: true,
          status: 'Active',
          sportIds: []
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    const request: UpdateCourtRequest = {
      name: 'Updated Court',
      description: 'Updated',
      surfaceType: 'Grass',
      indoor: true,
      sportIds: []
    };

    await updateCourt('complex-1', 'court-1', request, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/courts/court-1'),
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify(request)
      })
    );

    fetchSpy.mockRestore();
  });
});
