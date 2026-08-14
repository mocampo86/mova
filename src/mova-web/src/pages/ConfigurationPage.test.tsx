import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import ConfigurationPage from './ConfigurationPage';
import {
  useCancellationPolicy,
  useUpdateCancellationPolicy
} from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockPolicy = {
  sportsComplexId: 'complex-1',
  minimumHours: 24,
  allowUserCancellation: true
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
  const mutateAsync = vi.fn().mockResolvedValue(undefined);

  beforeEach(() => {
    cleanup();
    vi.resetAllMocks();
    mutateAsync.mockClear();
    window.history.pushState({}, '', '/');
  });

  function setupMocks(
    policyOverrides: Partial<ReturnType<typeof useCancellationPolicy>> = {},
    updateOverrides: Partial<ReturnType<typeof useUpdateCancellationPolicy>> = {}
  ) {
    vi.mocked(useCancellationPolicy).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: false,
      error: null,
      ...policyOverrides
    } as unknown as ReturnType<typeof useCancellationPolicy>);

    vi.mocked(useUpdateCancellationPolicy).mockReturnValue({
      mutateAsync,
      isPending: false,
      isSuccess: false,
      isError: false,
      error: null,
      ...updateOverrides
    } as unknown as ReturnType<typeof useUpdateCancellationPolicy>);
  }

  it('renders the configuration form with default values', () => {
    setupMocks();
    renderPage();

    expect(screen.getByRole('heading', { name: 'Configuration' })).toBeTruthy();

    const minimumHoursInput = screen.getByLabelText('Minimum cancellation notice (hours)') as HTMLInputElement;
    expect(minimumHoursInput.value).toBe('24');

    const switchInput = screen.getByLabelText('Allow users to cancel their reservations') as HTMLInputElement;
    expect(switchInput.checked).toBe(true);

    expect(screen.getByRole('button', { name: 'Save policy' })).toBeTruthy();
  });

  it('loads existing configuration values', () => {
    setupMocks({ data: { ...mockPolicy, minimumHours: 12, allowUserCancellation: false } });
    renderPage();

    const minimumHoursInput = screen.getByLabelText('Minimum cancellation notice (hours)') as HTMLInputElement;
    expect(minimumHoursInput.value).toBe('12');

    const switchInput = screen.getByLabelText('Allow users to cancel their reservations') as HTMLInputElement;
    expect(switchInput.checked).toBe(false);
  });

  it('submits the policy with updated values', async () => {
    setupMocks({ data: mockPolicy });
    renderPage();

    const minimumHoursInput = screen.getByLabelText('Minimum cancellation notice (hours)') as HTMLInputElement;
    fireEvent.change(minimumHoursInput, { target: { value: '6' } });

    const switchInput = screen.getByLabelText('Allow users to cancel their reservations') as HTMLInputElement;
    fireEvent.click(switchInput);

    fireEvent.click(screen.getByRole('button', { name: 'Save policy' }));

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalled();
    });

    expect(mutateAsync).toHaveBeenCalledWith({
      minimumHours: 6,
      allowUserCancellation: false
    });
  });

  it('shows an error when loading fails', () => {
    setupMocks({ isError: true, error: new Error('Load failed') });
    renderPage();

    expect(screen.getByText('Load failed')).toBeTruthy();
  });

  it('shows a success message after saving', async () => {
    setupMocks({ data: mockPolicy }, { isSuccess: true });
    renderPage();

    fireEvent.click(screen.getByRole('button', { name: 'Save policy' }));

    await waitFor(() => {
      expect(mutateAsync).toHaveBeenCalled();
    });

    expect(screen.getByText('The cancellation policy has been updated successfully.')).toBeTruthy();
  });
});
