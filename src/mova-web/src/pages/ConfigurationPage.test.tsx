import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import ConfigurationPage from './ConfigurationPage';
import {
  useCancellationPolicy,
  useRecurringReservationSettings,
  useUpdateCancellationPolicy,
  useUpdateRecurringReservationSettings
} from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockPolicy = {
  sportsComplexId: 'complex-1',
  minimumHours: 24,
  allowUserCancellation: true
};

const mockComplex = {
  id: 'complex-1',
  name: 'Test Complex',
  description: 'Description',
  address: 'Address',
  city: 'City',
  phoneNumber: '+1 234 567',
  email: 'test@example.com',
  status: 'Active',
  allowUserRecurringReservations: true,
  utcOffsetMinutes: 0,
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z'
};

function renderPage() {
  return renderWithAuth(
    <Routes>
      <Route
        path="/admin/complex/:complexId/configuration"
        element={<ConfigurationPage />}
      />
    </Routes>,
    { initialRoute: '/admin/complex/complex-1/configuration' }
  );
}

describe('ConfigurationPage', () => {
  const updatePolicyMutate = vi.fn().mockResolvedValue(undefined);
  const updateRecurringMutate = vi.fn().mockResolvedValue(undefined);

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    updatePolicyMutate.mockClear();
    updateRecurringMutate.mockClear();
    window.history.pushState({}, '', '/');
  });

  function setupMocks(
    policyOverrides: Partial<ReturnType<typeof useCancellationPolicy>> = {},
    updatePolicyOverrides: Partial<ReturnType<typeof useUpdateCancellationPolicy>> = {},
    recurringOverrides: Partial<ReturnType<typeof useRecurringReservationSettings>> = {},
    updateRecurringOverrides: Partial<ReturnType<typeof useUpdateRecurringReservationSettings>> = {}
  ) {
    vi.mocked(useCancellationPolicy).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
      error: null,
      ...policyOverrides
    } as unknown as ReturnType<typeof useCancellationPolicy>);

    vi.mocked(useUpdateCancellationPolicy).mockReturnValue({
      mutateAsync: updatePolicyMutate,
      isPending: false,
      isSuccess: false,
      isError: false,
      error: null,
      ...updatePolicyOverrides
    } as unknown as ReturnType<typeof useUpdateCancellationPolicy>);

    vi.mocked(useRecurringReservationSettings).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
      error: null,
      ...recurringOverrides
    } as unknown as ReturnType<typeof useRecurringReservationSettings>);

    vi.mocked(useUpdateRecurringReservationSettings).mockReturnValue({
      mutateAsync: updateRecurringMutate,
      isPending: false,
      isSuccess: false,
      isError: false,
      error: null,
      ...updateRecurringOverrides
    } as unknown as ReturnType<typeof useUpdateRecurringReservationSettings>);
  }

  it('renders the cancellation and recurring forms with default values', () => {
    setupMocks();
    renderPage();

    expect(screen.getByRole('heading', { name: 'Configuration' })).toBeTruthy();

    const minimumHoursInput = screen.getByLabelText('Minimum cancellation notice (hours)') as HTMLInputElement;
    expect(minimumHoursInput.value).toBe('24');

    const cancellationSwitch = screen.getByLabelText('Allow users to cancel their reservations') as HTMLInputElement;
    expect(cancellationSwitch.checked).toBe(true);

    const recurringSwitch = screen.getByLabelText('Allow regular users to create recurring reservations') as HTMLInputElement;
    expect(recurringSwitch.checked).toBe(true);

    expect(screen.getByRole('button', { name: 'Save policy' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Save recurring reservation settings' })).toBeTruthy();
  });

  it('loads existing cancellation and recurring values', () => {
    setupMocks(
      { data: { ...mockPolicy, minimumHours: 12, allowUserCancellation: false } },
      {},
      { data: { ...mockComplex, allowUserRecurringReservations: false } }
    );
    renderPage();

    const minimumHoursInput = screen.getByLabelText('Minimum cancellation notice (hours)') as HTMLInputElement;
    expect(minimumHoursInput.value).toBe('12');

    const cancellationSwitch = screen.getByLabelText('Allow users to cancel their reservations') as HTMLInputElement;
    expect(cancellationSwitch.checked).toBe(false);

    const recurringSwitch = screen.getByLabelText('Allow regular users to create recurring reservations') as HTMLInputElement;
    expect(recurringSwitch.checked).toBe(false);
  });

  it('submits the cancellation policy with updated values', async () => {
    setupMocks({ data: mockPolicy });
    renderPage();

    const minimumHoursInput = screen.getByLabelText('Minimum cancellation notice (hours)') as HTMLInputElement;
    fireEvent.change(minimumHoursInput, { target: { value: '6' } });

    const switchInput = screen.getByLabelText('Allow users to cancel their reservations') as HTMLInputElement;
    fireEvent.click(switchInput);

    fireEvent.click(screen.getByRole('button', { name: 'Save policy' }));

    await waitFor(() => {
      expect(updatePolicyMutate).toHaveBeenCalled();
    });

    expect(updatePolicyMutate).toHaveBeenCalledWith({
      minimumHours: 6,
      allowUserCancellation: false
    });
  });

  it('submits the recurring reservation settings with updated values', async () => {
    setupMocks({ data: mockPolicy }, {}, { data: mockComplex });
    renderPage();

    const recurringSwitch = screen.getByLabelText('Allow regular users to create recurring reservations') as HTMLInputElement;
    fireEvent.click(recurringSwitch);

    fireEvent.click(screen.getByRole('button', { name: 'Save recurring reservation settings' }));

    await waitFor(() => {
      expect(updateRecurringMutate).toHaveBeenCalled();
    });

    expect(updateRecurringMutate).toHaveBeenCalledWith({
      allowUserRecurringReservations: false
    });
  });

  it('shows an error when loading fails', () => {
    setupMocks({ isError: true, error: new Error('Load failed') });
    renderPage();

    expect(screen.getByText('Load failed')).toBeTruthy();
  });

  it('shows a success message after saving the cancellation policy', async () => {
    setupMocks({ data: mockPolicy }, { isSuccess: true });
    renderPage();

    fireEvent.click(screen.getByRole('button', { name: 'Save policy' }));

    await waitFor(() => {
      expect(updatePolicyMutate).toHaveBeenCalled();
    });

    expect(screen.getByText('The cancellation policy has been updated successfully.')).toBeTruthy();
  });

  it('shows a success message after saving the recurring reservation settings', async () => {
    setupMocks({ data: mockPolicy }, {}, { data: mockComplex }, { isSuccess: true });
    renderPage();

    fireEvent.click(screen.getByRole('button', { name: 'Save recurring reservation settings' }));

    await waitFor(() => {
      expect(updateRecurringMutate).toHaveBeenCalled();
    });

    expect(screen.getByText('The recurring reservation settings have been updated successfully.')).toBeTruthy();
  });
});
