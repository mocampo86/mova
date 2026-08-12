import { cleanup, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import UserHistoryPage from './UserHistoryPage';
import { useMyReservationHistory } from '../features/reservations/reservationApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/reservations/reservationApi');

const mockHistory = {
  items: [
    {
      id: 'reservation-history-1',
      complexId: 'complex-1',
      courtId: 'court-1',
      courtName: 'Court One',
      userId: 'user-1',
      userName: 'Test User',
      startAt: '2026-08-10T14:00:00Z',
      endAt: '2026-08-10T15:00:00Z',
      status: 'Confirmed',
      source: 'Web',
      notes: null,
      createdAt: '2026-08-01T12:00:00Z',
      cancelledAt: null,
      cancellationReason: null
    },
    {
      id: 'reservation-history-2',
      complexId: 'complex-1',
      courtId: 'court-2',
      courtName: 'Court Two',
      userId: 'user-1',
      userName: 'Test User',
      startAt: '2026-08-09T16:00:00Z',
      endAt: '2026-08-09T17:00:00Z',
      status: 'CancelledByAdmin',
      source: 'Web',
      notes: 'No show',
      createdAt: '2026-08-01T12:00:00Z',
      cancelledAt: '2026-08-09T15:00:00Z',
      cancellationReason: 'No show'
    }
  ],
  page: 1,
  pageSize: 10,
  totalItems: 2,
  totalPages: 1
};

function setupMocks(overrides: Partial<ReturnType<typeof useMyReservationHistory>> = {}) {
  vi.mocked(useMyReservationHistory).mockReturnValue({
    data: mockHistory,
    isLoading: false,
    isError: false,
    ...overrides
  } as unknown as ReturnType<typeof useMyReservationHistory>);
}

describe('UserHistoryPage', () => {
  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  it('renders the reservation history list with court and status', async () => {
    setupMocks();

    renderWithAuth(<UserHistoryPage />);

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Reservation history' })).toBeTruthy();
    });

    expect(screen.getByText('Court One')).toBeTruthy();
    expect(screen.getByText('Court Two')).toBeTruthy();
    expect(screen.getByText('Confirmed')).toBeTruthy();
    expect(screen.getByText('Cancelled by admin')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Book a court' })).toBeTruthy();
  });

  it('renders an empty state when no history exists', async () => {
    setupMocks({
      data: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 }
    } as unknown as ReturnType<typeof useMyReservationHistory>);

    renderWithAuth(<UserHistoryPage />);

    await waitFor(() => {
      expect(screen.getByText('No reservation history found.')).toBeTruthy();
    });
  });

  it('renders an error message when loading history fails', async () => {
    setupMocks({ data: undefined, isLoading: false, isError: true });

    renderWithAuth(<UserHistoryPage />);

    await waitFor(() => {
      expect(screen.getByText('Reservations could not be loaded.')).toBeTruthy();
    });
  });
});
