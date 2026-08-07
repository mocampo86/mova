import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { Route, Routes } from 'react-router-dom';
import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import ComplexAdminLayout from './ComplexAdminLayout';
import { useComplexDashboard } from '../features/complexes/complexApi';
import { renderWithAuth, type RenderWithAuthOptions } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockDashboard = {
  complex: {
    id: 'complex-id',
    name: 'Test Complex',
    status: 'Active',
    lastUpdatedAt: null
  },
  courts: { active: 0, inactive: 0 },
  reservationsToday: { confirmed: 0, cancelled: 0, completed: 0 },
  blockedUsers: 0
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
    vi.mocked(useComplexDashboard).mockReturnValue({
      data: mockDashboard,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useComplexDashboard>);

    renderLayout('/admin/complex/complex-id');

    expect(screen.getByText('Test Complex')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Profile' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Courts' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Reservations' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Users' })).toBeTruthy();
  });

  it('renders navigation links scoped to the current complex', () => {
    vi.mocked(useComplexDashboard).mockReturnValue({
      data: mockDashboard,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useComplexDashboard>);

    renderLayout('/admin/complex/complex-1');

    const dashboardLink = screen.getByRole('link', { name: 'Dashboard' });
    expect(dashboardLink.getAttribute('href')).toBe('/admin/complex/complex-1');

    const courtsLink = screen.getByRole('link', { name: 'Courts' });
    expect(courtsLink.getAttribute('href')).toBe('/admin/complex/complex-1/courts');

    const reservationsLink = screen.getByRole('link', { name: 'Reservations' });
    expect(reservationsLink.getAttribute('href')).toBe('/admin/complex/complex-1/reservations');

    const usersLink = screen.getByRole('link', { name: 'Users' });
    expect(usersLink.getAttribute('href')).toBe('/admin/complex/complex-1/users');
  });

  it('marks the Dashboard link as active on the root admin route', () => {
    vi.mocked(useComplexDashboard).mockReturnValue({
      data: mockDashboard,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useComplexDashboard>);

    renderLayout('/admin/complex/complex-id');

    expect(screen.getByRole('link', { name: 'Dashboard' }).getAttribute('aria-current')).toBe('page');
    expect(screen.getByRole('link', { name: 'Courts' }).getAttribute('aria-current')).toBeNull();
  });

  it('updates the active link when navigating to a section', async () => {
    vi.mocked(useComplexDashboard).mockReturnValue({
      data: mockDashboard,
      isLoading: false,
      isError: false
    } as unknown as ReturnType<typeof useComplexDashboard>);

    renderLayout('/admin/complex/complex-id');

    fireEvent.click(screen.getByRole('link', { name: 'Courts' }));

    await waitFor(() => {
      expect(window.location.pathname).toBe('/admin/complex/complex-id/courts');
    });

    expect(screen.getByRole('link', { name: 'Courts' }).getAttribute('aria-current')).toBe('page');
    expect(screen.getByRole('link', { name: 'Dashboard' }).getAttribute('aria-current')).toBeNull();
    expect(screen.getByTestId('section')).toBeTruthy();
  });
});
