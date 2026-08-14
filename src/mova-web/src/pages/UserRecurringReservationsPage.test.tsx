import { screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import UserRecurringReservationsPage from './UserRecurringReservationsPage';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi', () => ({
  useActiveComplexes: () => ({
    data: {
      items: [
        {
          id: 'complex-1',
          name: 'Mova Club'
        }
      ]
    },
    isLoading: false,
    isError: false
  }),
  useActiveCourts: () => ({
    data: {
      items: [
        {
          id: 'court-1',
          name: 'Court 1'
        }
      ]
    },
    isLoading: false,
    isError: false
  })
}));

vi.mock('../features/reservations/reservationApi', () => ({
  useCreateMyRecurringReservation: () => ({
    mutateAsync: vi.fn(),
    reset: vi.fn(),
    isPending: false,
    isError: false,
    isSuccess: false,
    error: null,
    data: null
  })
}));

describe('UserRecurringReservationsPage', () => {
  it('renders the recurring reservation form', () => {
    renderWithAuth(<UserRecurringReservationsPage />, {
      authState: {
        accessToken: 'token',
        isAuthenticated: true
      },
      initialRoute: '/user/recurring'
    });

    expect(screen.getByRole('heading', { name: 'Recurring bookings' })).toBeTruthy();
    expect(screen.getByLabelText('Sports complex')).toBeTruthy();
    expect(screen.getByLabelText('Court')).toBeTruthy();
    expect(screen.getByLabelText('Day of week')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Create recurring booking' })).toBeTruthy();
  });
});
