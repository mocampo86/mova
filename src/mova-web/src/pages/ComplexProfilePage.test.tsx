import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { Routes, Route } from 'react-router-dom';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import ComplexProfilePage from './ComplexProfilePage';
import { useAdminComplex, useUpdateComplex } from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockComplex = {
  id: 'complex-id',
  name: 'Test Complex',
  description: 'A test complex',
  address: 'Test Address',
  city: 'Test City',
  phoneNumber: '+54 11 1234 5678',
  email: 'test@complex.com',
  latitude: -34.6,
  longitude: -58.4,
  status: 'Active',
  allowUserRecurringReservations: false,
  timeZoneId: 'America/Montevideo',
  createdAt: '2026-08-01T12:00:00Z',
  updatedAt: '2026-08-06T12:00:00Z'
};

function renderPage() {
  return renderWithAuth(
    <Routes>
      <Route path="/admin/complex/:complexId/profile" element={<ComplexProfilePage />} />
    </Routes>,
    { initialRoute: '/admin/complex/complex-id/profile' }
  );
}

describe('ComplexProfilePage', () => {
  const mutateMock = vi.fn();

  beforeEach(() => {
    vi.resetAllMocks();
    vi.mocked(useAdminComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);
    vi.mocked(useUpdateComplex).mockReturnValue({
      mutate: mutateMock,
      isPending: false,
      error: null,
      isSuccess: false,
      reset: vi.fn()
    } as unknown as ReturnType<typeof useUpdateComplex>);
  });

  afterEach(() => {
    cleanup();
  });

  it('renders the edit form pre-populated with the current complex data', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByDisplayValue('Test Complex')).toBeTruthy();
    });

    expect(screen.getByDisplayValue('A test complex')).toBeTruthy();
    expect(screen.getByDisplayValue('Test Address')).toBeTruthy();
    expect(screen.getByDisplayValue('Test City')).toBeTruthy();
    expect(screen.getByDisplayValue('+54 11 1234 5678')).toBeTruthy();
    expect(screen.getByDisplayValue('test@complex.com')).toBeTruthy();
    expect(screen.getByDisplayValue('Active')).toBeTruthy();
    expect(screen.getByText(/Last updated/i)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Save changes' })).toBeTruthy();
  });

  it('displays validation errors for invalid fields', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByDisplayValue('+54 11 1234 5678')).toBeTruthy();
    });

    const phoneInput = screen.getByDisplayValue('+54 11 1234 5678');
    fireEvent.change(phoneInput, { target: { value: '12345678' } });

    fireEvent.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(screen.getByText(/Phone number must be in international format/i)).toBeTruthy();
    });
  });

  it('calls the update mutation with the form values on submit', async () => {
    renderPage();

    await waitFor(() => {
      expect(screen.getByDisplayValue('Test Complex')).toBeTruthy();
    });

    fireEvent.click(screen.getByRole('button', { name: 'Save changes' }));

    await waitFor(() => {
      expect(mutateMock).toHaveBeenCalledOnce();
      expect(mutateMock).toHaveBeenCalledWith({
        name: 'Test Complex',
        description: 'A test complex',
        address: 'Test Address',
        city: 'Test City',
        phoneNumber: '+54 11 1234 5678',
        email: 'test@complex.com',
        latitude: -34.6,
        longitude: -58.4,
        timeZoneId: 'America/Montevideo'
      });
    });
  });

  it('renders loading state while the complex is loading', () => {
    vi.mocked(useAdminComplex).mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderPage();

    expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
  });

  it('renders error state when the complex cannot be loaded', () => {
    vi.mocked(useAdminComplex).mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderPage();

    expect(screen.getByText(/complex profile could not be loaded/i)).toBeTruthy();
  });

  it('displays the mutation error message when update fails', async () => {
    vi.mocked(useUpdateComplex).mockReturnValue({
      mutate: mutateMock,
      isPending: false,
      error: new Error('Update failed'),
      isSuccess: false,
      reset: vi.fn()
    } as unknown as ReturnType<typeof useUpdateComplex>);

    renderPage();

    await waitFor(() => {
      expect(screen.getByText('Update failed')).toBeTruthy();
    });
  });
});
