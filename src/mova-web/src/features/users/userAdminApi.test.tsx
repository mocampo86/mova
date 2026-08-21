import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthContext } from '../auth/AuthContext';
import type { AuthState } from '../auth/authTypes';
import {
  useBlockUser,
  useComplexUsers,
  useSearchUsers,
  useUnblockUser,
  useUserReservations
} from './userAdminApi';
import type { BlockUserRequest, UserListFilters, UserReservationFilters } from './userAdminTypes';

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

const emptyPagedResult = {
  items: [],
  page: 1,
  pageSize: 10,
  totalItems: 0,
  totalPages: 0
};

describe('useComplexUsers', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify(emptyPagedResult), {
        status: 200,
        headers: { 'Content-Type': 'application/json' }
      })
    );
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it('fetches the complex users endpoint with a 1-based page number', async () => {
    const filters: UserListFilters = {
      page: 0,
      pageSize: 10,
      search: 'test',
      sort: 'fullName:asc'
    };

    const { result } = renderHook(() => useComplexUsers('complex-1', filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/users?page=1'),
      expect.any(Object)
    );
  });
});

describe('useSearchUsers', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify(emptyPagedResult), {
        status: 200,
        headers: { 'Content-Type': 'application/json' }
      })
    );
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it('fetches the user search endpoint with a 1-based page number', async () => {
    const filters: UserListFilters = {
      page: 0,
      pageSize: 10,
      search: 'test',
      sort: 'fullName:asc'
    };

    const { result } = renderHook(() => useSearchUsers('complex-1', filters, true), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/users/search?page=1&pageSize=10&sort=fullName%3Aasc&search=test'),
      expect.any(Object)
    );
  });

  it('does not fetch when disabled', async () => {
    const filters: UserListFilters = {
      page: 0,
      pageSize: 10,
      search: 'test',
      sort: 'fullName:asc'
    };

    renderHook(() => useSearchUsers('complex-1', filters, false), {
      wrapper: createWrapper()
    });

    expect(fetchSpy).not.toHaveBeenCalled();
  });
});

describe('useUserReservations', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify(emptyPagedResult), {
        status: 200,
        headers: { 'Content-Type': 'application/json' }
      })
    );
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it('fetches the user reservations endpoint', async () => {
    const filters: UserReservationFilters = {
      page: 0,
      pageSize: 10,
      sort: 'startAt:desc'
    };

    const { result } = renderHook(() => useUserReservations('complex-1', 'user-1', filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/users/user-1/reservations'),
      expect.any(Object)
    );
  });
});

describe('useBlockUser', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'block-1',
          sportsComplexId: 'complex-1',
          userId: 'user-1',
          reason: 'Spam',
          blockedAt: '2026-08-01T12:00:00Z',
          blockedUntil: null,
          blockedByUserId: 'admin-1',
          status: 'Active'
        }),
        {
          status: 201,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );
  });

  afterEach(() => {
    fetchSpy.mockRestore();
  });

  it('sends a POST request to the block endpoint', async () => {
    const { result } = renderHook(() => useBlockUser('complex-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    const request: BlockUserRequest = {
      userId: 'user-1',
      reason: 'Spam'
    };

    await result.current.mutateAsync(request);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/blocked-users'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(request)
      })
    );
  });
});

describe('useUnblockUser', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'block-1',
          sportsComplexId: 'complex-1',
          userId: 'user-1',
          reason: 'Spam',
          blockedAt: '2026-08-01T12:00:00Z',
          blockedUntil: null,
          blockedByUserId: 'admin-1',
          status: 'Lifted'
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

  it('sends a DELETE request to the unblock endpoint', async () => {
    const { result } = renderHook(() => useUnblockUser('complex-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync('block-1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/blocked-users/block-1'),
      expect.objectContaining({
        method: 'DELETE'
      })
    );
  });
});
