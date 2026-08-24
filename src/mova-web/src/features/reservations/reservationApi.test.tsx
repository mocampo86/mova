import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthContext } from '../auth/AuthContext';
import type { AuthState } from '../auth/authTypes';
import {
  cancelMyReservation,
  cancelRecurringReservation,
  cancelReservation,
  createMyRecurringReservation,
  createReservation,
  getRecurringReservations,
  updateReservationStatus,
  useCancelMyReservation,
  useCancelRecurringReservation,
  useCancelReservation,
  useCreateMyRecurringReservation,
  useCreateReservation,
  useMyReservationHistory,
  useMyReservations,
  useRecurringReservations,
  useReservations,
  useUpdateReservationStatus
} from './reservationApi';
import type {
  CreateMyRecurringReservationRequest,
  CreateReservationRequest,
  RecurringReservationListFilters,
  ReservationListFilters,
  UpdateReservationStatusRequest,
  UserReservationsFilters
} from './reservationTypes';

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

describe('useReservations', () => {
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

  it('sends a 1-based page number and filters to the list endpoint', async () => {
    const filters: ReservationListFilters = {
      page: 0,
      pageSize: 10,
      courtId: 'court-1',
      status: 'Confirmed',
      date: '2026-08-10'
    };

    const { result } = renderHook(() => useReservations('complex-1', filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/reservations?'),
      expect.any(Object)
    );

    const url = fetchSpy.mock.calls[0][0] as string;
    expect(url).toContain('page=1');
    expect(url).toContain('courtId=court-1');
    expect(url).toContain('status=Confirmed');
    expect(url).toContain('date=2026-08-10');
    expect(url).not.toContain('utcOffsetMinutes');
    expect(url).toContain('sort=startAt%3Adesc');
  });
});

describe('useCreateReservation', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          complexId: 'complex-1',
          courtId: 'court-1',
          userId: 'user-1',
          startAt: '2026-08-10T14:00:00Z',
          endAt: '2026-08-10T15:00:00Z',
          status: 'Confirmed',
          source: 'Admin'
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

  it('sends a POST request to the reservations endpoint', async () => {
    const request: CreateReservationRequest = {
      courtId: 'court-1',
      userId: 'user-1',
      startAt: '2026-08-10T14:00:00Z',
      endAt: '2026-08-10T15:00:00Z'
    };

    const { result } = renderHook(() => useCreateReservation('complex-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync(request);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/reservations'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(request)
      })
    );
  });
});

describe('createReservation', () => {
  it('sends a POST request with the given payload', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          complexId: 'complex-1',
          courtId: 'court-1',
          userId: 'user-1',
          startAt: '2026-08-10T14:00:00Z',
          endAt: '2026-08-10T15:00:00Z',
          status: 'Confirmed',
          source: 'Admin'
        }),
        {
          status: 201,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    const request: CreateReservationRequest = {
      courtId: 'court-1',
      userId: 'user-1',
      startAt: '2026-08-10T14:00:00Z',
      endAt: '2026-08-10T15:00:00Z'
    };

    await createReservation('complex-1', request, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/reservations'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(request)
      })
    );

    fetchSpy.mockRestore();
  });
});

describe('createMyRecurringReservation', () => {
  it('sends a POST request to the user recurring reservations endpoint', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'recurring-1',
          complexId: 'complex-1',
          courtId: 'court-1',
          userId: 'user-1',
          dayOfWeek: 1,
          startTime: '14:00:00',
          durationMinutes: 60,
          startDate: '2026-08-10',
          endDate: '2026-08-31',
          status: 'Active',
          createdAt: '2026-08-01T12:00:00Z',
          occurrences: []
        }),
        {
          status: 201,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    const request: CreateMyRecurringReservationRequest = {
      courtId: 'court-1',
      dayOfWeek: 1,
      startTime: '14:00:00',
      durationMinutes: 60,
      startDate: '2026-08-10',
      endDate: '2026-08-31'
    };

    await createMyRecurringReservation('complex-1', request, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/recurring-reservations/me'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(request)
      })
    );

    fetchSpy.mockRestore();
  });
});

describe('useCreateMyRecurringReservation', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'recurring-1',
          complexId: 'complex-1',
          courtId: 'court-1',
          userId: 'user-1',
          dayOfWeek: 1,
          startTime: '14:00:00',
          durationMinutes: 60,
          startDate: '2026-08-10',
          endDate: '2026-08-31',
          status: 'Active',
          createdAt: '2026-08-01T12:00:00Z',
          occurrences: []
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

  it('creates a user recurring reservation with the given payload', async () => {
    const request: CreateMyRecurringReservationRequest = {
      courtId: 'court-1',
      dayOfWeek: 1,
      startTime: '14:00:00',
      durationMinutes: 60,
      startDate: '2026-08-10',
      endDate: '2026-08-31'
    };

    const { result } = renderHook(() => useCreateMyRecurringReservation('complex-1'), {
      wrapper: createWrapper()
    });

    await result.current.mutateAsync(request);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/recurring-reservations/me'),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(request)
      })
    );
  });
});

describe('useCancelReservation', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          status: 'CancelledByAdmin'
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

  it('sends a PATCH request to the cancel endpoint', async () => {
    const { result } = renderHook(() => useCancelReservation('complex-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync({
      reservationId: 'reservation-1',
      request: { reason: 'No show' }
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/reservations/reservation-1/cancel'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify({ reason: 'No show' })
      })
    );
  });
});

describe('cancelReservation', () => {
  it('sends a PATCH request to the cancel endpoint', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          status: 'CancelledByAdmin'
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    await cancelReservation('complex-1', 'reservation-1', { reason: 'No show' }, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/reservations/reservation-1/cancel'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify({ reason: 'No show' })
      })
    );

    fetchSpy.mockRestore();
  });
});

describe('useCancelMyReservation', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          status: 'CancelledByUser'
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

  it('sends a PATCH request to the user cancel endpoint', async () => {
    const { result } = renderHook(() => useCancelMyReservation(), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync({
      reservationId: 'reservation-1',
      request: { reason: 'Changed plans' }
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/users/me/reservations/reservation-1/cancel'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify({ reason: 'Changed plans' })
      })
    );
  });
});

describe('cancelMyReservation', () => {
  it('sends a PATCH request to the user cancel endpoint', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          status: 'CancelledByUser'
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    await cancelMyReservation('reservation-1', { reason: 'Changed plans' }, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/users/me/reservations/reservation-1/cancel'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify({ reason: 'Changed plans' })
      })
    );

    fetchSpy.mockRestore();
  });
});

describe('useUpdateReservationStatus', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          status: 'Completed'
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

  it('sends a PATCH request to the status endpoint', async () => {
    const request: UpdateReservationStatusRequest = { status: 'Completed' };

    const { result } = renderHook(() => useUpdateReservationStatus('complex-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync({
      reservationId: 'reservation-1',
      request
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/reservations/reservation-1/status'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify(request)
      })
    );
  });
});

describe('updateReservationStatus', () => {
  it('sends a PATCH request to the status endpoint', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'reservation-1',
          status: 'Completed'
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    const request: UpdateReservationStatusRequest = { status: 'NoShow' };

    await updateReservationStatus('complex-1', 'reservation-1', request, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/reservations/reservation-1/status'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify(request)
      })
    );

    fetchSpy.mockRestore();
  });
});

describe('useMyReservations', () => {
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

  it('sends a 1-based page number to the user reservations endpoint', async () => {
    const filters: UserReservationsFilters = { page: 0, pageSize: 10 };

    const { result } = renderHook(() => useMyReservations(filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/users/me/reservations?'),
      expect.any(Object)
    );

    const url = fetchSpy.mock.calls[0][0] as string;
    expect(url).toContain('page=1');
    expect(url).toContain('pageSize=10');
  });
});

describe('useMyReservationHistory', () => {
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

  it('sends a 1-based page number to the user reservation history endpoint', async () => {
    const filters: UserReservationsFilters = { page: 0, pageSize: 10 };

    const { result } = renderHook(() => useMyReservationHistory(filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/users/me/reservations/history?'),
      expect.any(Object)
    );

    const url = fetchSpy.mock.calls[0][0] as string;
    expect(url).toContain('page=1');
    expect(url).toContain('pageSize=10');
  });
});

describe('getRecurringReservations', () => {
  it('sends a GET request with 1-based page and filters', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
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

    const filters: RecurringReservationListFilters = {
      page: 0,
      pageSize: 10,
      courtId: 'court-1',
      status: 'Active',
      sort: 'createdAt:desc'
    };

    await getRecurringReservations('complex-1', filters, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/recurring-reservations?'),
      expect.any(Object)
    );

    const url = fetchSpy.mock.calls[0][0] as string;
    expect(url).toContain('page=1');
    expect(url).toContain('pageSize=10');
    expect(url).toContain('courtId=court-1');
    expect(url).toContain('status=Active');
    expect(url).toContain('sort=createdAt%3Adesc');

    fetchSpy.mockRestore();
  });
});

describe('useRecurringReservations', () => {
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

  it('sends a 1-based page number and filters to the recurring list endpoint', async () => {
    const filters: RecurringReservationListFilters = {
      page: 1,
      pageSize: 10,
      status: 'Active',
      sort: 'createdAt:desc'
    };

    const { result } = renderHook(() => useRecurringReservations('complex-1', filters), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/recurring-reservations?'),
      expect.any(Object)
    );

    const url = fetchSpy.mock.calls[0][0] as string;
    expect(url).toContain('page=2');
    expect(url).toContain('status=Active');
    expect(url).toContain('sort=createdAt%3Adesc');
  });
});

describe('cancelRecurringReservation', () => {
  it('sends a PATCH request to the recurring cancel endpoint', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'recurring-1',
          status: 'Cancelled'
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    await cancelRecurringReservation('complex-1', 'recurring-1', { reason: 'No longer needed' }, 'token');

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/recurring-reservations/recurring-1/cancel'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify({ reason: 'No longer needed' })
      })
    );

    fetchSpy.mockRestore();
  });
});

describe('useCancelRecurringReservation', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'recurring-1',
          status: 'Cancelled'
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

  it('sends a PATCH request to the recurring cancel endpoint', async () => {
    const { result } = renderHook(() => useCancelRecurringReservation('complex-1'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync({
      recurringReservationId: 'recurring-1',
      request: { reason: 'No longer needed' }
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(fetchSpy).toHaveBeenCalledWith(
      expect.stringContaining('/api/v1/complexes/complex-1/recurring-reservations/recurring-1/cancel'),
      expect.objectContaining({
        method: 'PATCH',
        body: JSON.stringify({ reason: 'No longer needed' })
      })
    );
  });
});
