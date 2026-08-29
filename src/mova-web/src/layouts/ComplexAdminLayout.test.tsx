import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Route, Routes } from 'react-router-dom';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import ComplexAdminLayout from './ComplexAdminLayout';
import { useAdminComplex } from '../features/complexes/complexApi';
import { STORAGE_KEY } from '../i18n/languageStorage';
import { renderWithAuth, type RenderWithAuthOptions } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockComplex = {
  id: 'complex-id',
  name: 'Test Complex',
  description: 'A test complex',
  address: 'Test Address',
  city: 'Test City',
  phoneNumber: '+598 99 123 456',
  email: 'test@complex.com',
  status: 'Active',
  allowUserRecurringReservations: true,
  timeZoneId: 'America/Montevideo',
  updatedAt: null
};

function renderLayout(
  initialRoute: string,
  authState?: RenderWithAuthOptions['authState']
) {
  return renderWithAuth(
    <Routes>
      <Route path="/admin/complex/:complexId" element={<ComplexAdminLayout />}>
        <Route index element={<div data-testid="dashboard">Dashboard</div>} />
        <Route path="*" element={<div data-testid="section">Section</div>} />
      </Route>
    </Routes>,
    {
      initialRoute,
      authState
    }
  );
}

describe('ComplexAdminLayout', () => {
  afterEach(cleanup);

  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders the complex name in the header and navigation links', () => {
    vi.mocked(useAdminComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderLayout('/admin/complex/complex-id');

    expect(screen.getByText('Test Complex')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Profile' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Business hours' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Courts' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Reservations' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Users' })).toBeTruthy();
  });

  it('renders navigation links scoped to the current complex', () => {
    vi.mocked(useAdminComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderLayout('/admin/complex/complex-1');

    const dashboardLink = screen.getByRole('link', { name: 'Dashboard' });
    expect(dashboardLink.getAttribute('href')).toBe('/admin/complex/complex-1');

    const businessHoursLink = screen.getByRole('link', { name: 'Business hours' });
    expect(businessHoursLink.getAttribute('href')).toBe('/admin/complex/complex-1/business-hours');

    const courtsLink = screen.getByRole('link', { name: 'Courts' });
    expect(courtsLink.getAttribute('href')).toBe('/admin/complex/complex-1/courts');

    const reservationsLink = screen.getByRole('link', { name: 'Reservations' });
    expect(reservationsLink.getAttribute('href')).toBe('/admin/complex/complex-1/reservations');

    const usersLink = screen.getByRole('link', { name: 'Users' });
    expect(usersLink.getAttribute('href')).toBe('/admin/complex/complex-1/users');
  });

  it('marks the Dashboard link as active on the root admin route', () => {
    vi.mocked(useAdminComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderLayout('/admin/complex/complex-id');

    expect(screen.getByRole('link', { name: 'Dashboard' }).getAttribute('aria-current')).toBe('page');
    expect(screen.getByRole('link', { name: 'Courts' }).getAttribute('aria-current')).toBeNull();
  });

  it('updates the active link when navigating to a section', async () => {
    vi.mocked(useAdminComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderLayout('/admin/complex/complex-id');

    fireEvent.click(screen.getByRole('link', { name: 'Courts' }));

    await waitFor(() => {
      expect(window.location.pathname).toBe('/admin/complex/complex-id/courts');
    });

    expect(screen.getByRole('link', { name: 'Courts' }).getAttribute('aria-current')).toBe('page');
    expect(screen.getByRole('link', { name: 'Dashboard' }).getAttribute('aria-current')).toBeNull();
    expect(screen.getByTestId('section')).toBeTruthy();
  });

  it('renders the language selector in the admin header', () => {
    vi.mocked(useAdminComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderLayout('/admin/complex/complex-id');

    expect(screen.getByRole('combobox', { name: 'Language' })).toBeTruthy();
  });

  it('updates the admin language and persists the selection when a new language is chosen', async () => {
    const user = userEvent.setup();
    window.localStorage.removeItem(STORAGE_KEY);

    vi.mocked(useAdminComplex).mockReturnValue({
      data: mockComplex,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useAdminComplex>);

    renderLayout('/admin/complex/complex-id');

    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeTruthy();

    await user.click(screen.getByRole('combobox', { name: 'Language' }));
    await user.click(screen.getByRole('option', { name: 'Español' }));

    await waitFor(() => {
      expect(screen.getByRole('link', { name: 'Panel' })).toBeTruthy();
    });

    expect(window.localStorage.getItem(STORAGE_KEY)).toBe('es');
  });
});
