import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import UserRecurringReservationsPage from './UserRecurringReservationsPage';
import { renderWithAuth } from '../test-utils';

const mockComplex = {
  id: 'complex-1',
  name: 'Mova Club',
  description: 'Description',
  address: 'Address',
  city: 'City',
  phoneNumber: '+1 234 567',
  email: 'test@example.com',
  allowUserRecurringReservations: true
};

const createMutation = {
  mutateAsync: vi.fn(),
  reset: vi.fn(),
  isPending: false,
  isError: false,
  isSuccess: false,
  error: null,
  data: null
};

vi.mock('../features/complexes/complexApi', () => ({
  useActiveComplexes: () => ({
    data: {
      items: [mockComplex]
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
  }),
  useRecurringReservationSettings: vi.fn()
}));

vi.mock('../features/reservations/reservationApi', () => ({
  useCreateMyRecurringReservation: () => createMutation
}));

import { useRecurringReservationSettings } from '../features/complexes/complexApi';

function setSelectValue(testId: string, value: string) {
  const selectRoot = screen.getByTestId(testId);
  const input = selectRoot.querySelector('input.MuiSelect-nativeInput') as HTMLInputElement;
  fireEvent.change(input, { target: { value } });
}

describe('UserRecurringReservationsPage', () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.resetAllMocks();
    createMutation.mutateAsync.mockClear();
  });

  function setupSettings(allowUserRecurringReservations: boolean) {
    vi.mocked(useRecurringReservationSettings).mockReturnValue({
      data: { ...mockComplex, allowUserRecurringReservations },
      isLoading: false,
      isError: false,
      error: null
    } as unknown as ReturnType<typeof useRecurringReservationSettings>);
  }

  it('renders the recurring reservation form when the setting is enabled', () => {
    setupSettings(true);
    renderWithAuth(<UserRecurringReservationsPage />, {
      authState: {
        accessToken: 'token',
        isAuthenticated: true
      },
      initialRoute: '/user/recurring'
    });

    expect(screen.getByRole('heading', { name: 'Recurring bookings' })).toBeTruthy();

    setSelectValue('recurring-complex-select', 'complex-1');

    expect(screen.getByTestId('recurring-court-select')).toBeTruthy();
    expect(screen.getByRole('combobox', { name: 'Day of week' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Create recurring booking' })).toBeTruthy();
  });

  it('disables the form and shows a message when the setting is disabled', () => {
    setupSettings(false);
    renderWithAuth(<UserRecurringReservationsPage />, {
      authState: {
        accessToken: 'token',
        isAuthenticated: true
      },
      initialRoute: '/user/recurring'
    });

    expect(screen.getByRole('heading', { name: 'Recurring bookings' })).toBeTruthy();

    setSelectValue('recurring-complex-select', 'complex-1');

    expect(screen.getByText('Recurring reservations are not available for this complex.')).toBeTruthy();

    const createButton = screen.getByRole('button', { name: 'Create recurring booking' }) as HTMLButtonElement;
    expect(createButton.disabled).toBe(true);
  });

  it('submits the form when the setting is enabled', async () => {
    setupSettings(true);
    renderWithAuth(<UserRecurringReservationsPage />, {
      authState: {
        accessToken: 'token',
        isAuthenticated: true
      },
      initialRoute: '/user/recurring'
    });

    setSelectValue('recurring-complex-select', 'complex-1');
    setSelectValue('recurring-court-select', 'court-1');

    fireEvent.click(screen.getByRole('button', { name: 'Create recurring booking' }));

    await waitFor(() => {
      expect(createMutation.mutateAsync).toHaveBeenCalled();
    });

    expect(createMutation.mutateAsync).toHaveBeenCalledWith(expect.objectContaining({
      courtId: 'court-1'
    }));
  });
});
