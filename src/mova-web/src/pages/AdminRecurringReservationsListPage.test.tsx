import { cleanup, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Routes, Route } from 'react-router-dom';
import AdminRecurringReservationsListPage from './AdminRecurringReservationsListPage';
import { useRecurringReservations } from '../features/reservations/reservationApi';
import { useCourts } from '../features/courts/courtApi';
import { useComplexUsers } from '../features/users/userAdminApi';
import { renderWithAuth } from '../test-utils';
import type { RecurringReservationListItem } from '../features/reservations/reservationTypes';

vi.mock('../features/reservations/reservationApi');
vi.mock('../features/courts/courtApi');
vi.mock('../features/users/userAdminApi');

const mockRecurringReservations = {
  items: [
    {
      id: 'recurring-1',
      complexId: 'complex-1',
      courtId: 'court-1',
      courtName: 'Court One',
      userId: 'user-1',
      userName: 'Test User',
      dayOfWeek: 1,
      startTime: '14:00:00',
      durationMinutes: 60,
      startDate: '2026-08-10',
      endDate: '2026-08-31',
      status: 'Active',
      createdAt: '2026-08-01T12:00:00Z',
      updatedAt: null
    } as RecurringReservationListItem
  ],
  page: 1,
  pageSize: 10,
  totalItems: 1,
  totalPages: 1
};

const mockCourts = {
  items: [
    {
      id: 'court-1',
      sportsComplexId: 'complex-1',
      name: 'Court One',
      description: '',
      surfaceType: 'Synthetic',
      indoor: true,
      status: 'Active',
      sportIds: ['sport-1'],
      createdAt: '2026-08-01T12:00:00Z',
      updatedAt: null
    }
  ],
  page: 1,
  pageSize: 100,
  totalItems: 1,
  totalPages: 1
};

const mockUsers = {
  items: [
    {
      id: 'user-1',
      email: 'user@example.com',
      fullName: 'Test User',
      phoneNumber: '+598 99 123 456',
      phoneVerified: false,
      isBlocked: false,
      blockId: null,
      blockReason: null,
      blockedUntil: null
    }
  ],
  page: 1,
  pageSize: 100,
  totalItems: 1,
  totalPages: 1
};

describe('AdminRecurringReservationsListPage', () => {
  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
  });

  function setupMocks() {
    vi.mocked(useRecurringReservations).mockReturnValue({
      data: mockRecurringReservations,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useRecurringReservations>);

    vi.mocked(useCourts).mockReturnValue({
      data: mockCourts,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useCourts>);

    vi.mocked(useComplexUsers).mockReturnValue({
      data: mockUsers,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useComplexUsers>);
  }

  it('renders the recurring reservations list with details and actions', async () => {
    setupMocks();

    renderWithAuth(
      <Routes>
        <Route
          path="/admin/complex/:complexId/recurring"
          element={<AdminRecurringReservationsListPage />}
        />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/recurring' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Recurring bookings' })).toBeTruthy();
    });

    expect(screen.getByText('Court One')).toBeTruthy();
    expect(screen.getByText('Test User')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Cancel series' })).toBeTruthy();
    expect(screen.getByText('New recurring booking')).toBeTruthy();
  });

  it('renders an empty state when no recurring reservations exist', async () => {
    vi.mocked(useRecurringReservations).mockReturnValue({
      data: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useRecurringReservations>);

    vi.mocked(useCourts).mockReturnValue({
      data: mockCourts,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useCourts>);

    vi.mocked(useComplexUsers).mockReturnValue({
      data: { items: [], page: 1, pageSize: 100, totalItems: 0, totalPages: 0 },
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useComplexUsers>);

    renderWithAuth(
      <Routes>
        <Route
          path="/admin/complex/:complexId/recurring"
          element={<AdminRecurringReservationsListPage />}
        />
      </Routes>,
      { initialRoute: '/admin/complex/complex-1/recurring' }
    );

    await waitFor(() => {
      expect(screen.getByText('There are no recurring bookings for this complex.')).toBeTruthy();
    });
  });
});
