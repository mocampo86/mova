import { screen, waitFor, within } from '@testing-library/react';
import { Routes, Route } from 'react-router-dom';
import { describe, expect, it, vi, beforeEach } from 'vitest';
import ComplexAdminPage from './ComplexAdminPage';
import { useComplexDashboard } from '../features/complexes/complexApi';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/complexes/complexApi');

const mockDashboard = {
  complex: {
    id: 'complex-id',
    name: 'Test Complex',
    status: 'Active',
    lastUpdatedAt: '2026-08-06T12:00:00Z'
  },
  courts: { active: 2, inactive: 1 },
  reservationsToday: { confirmed: 5, cancelled: 1, completed: 3 },
  blockedUsers: 2
};

describe('ComplexAdminPage', () => {
  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('renders complex dashboard with counts and links', async () => {
    vi.mocked(useComplexDashboard).mockReturnValue(
      { data: mockDashboard, isLoading: false, isError: false } as unknown as ReturnType<typeof useComplexDashboard>
    );

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId" element={<ComplexAdminPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-id' }
    );

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'Test Complex' })).toBeTruthy();
    });

    expect(screen.getByText(/Last updated/i)).toBeTruthy();

    const links = screen.getAllByRole('link');

    const activeCourtsLink = links.find((link) => link.textContent?.includes('Active courts'))!;
    expect(activeCourtsLink.getAttribute('href')).toBe('/admin/complex/complex-id/courts');
    expect(within(activeCourtsLink).getByText('2')).toBeTruthy();

    const inactiveCourtsLink = links.find((link) => link.textContent?.includes('Inactive courts'))!;
    expect(inactiveCourtsLink.getAttribute('href')).toBe('/admin/complex/complex-id/courts');
    expect(within(inactiveCourtsLink).getByText('1')).toBeTruthy();

    const confirmedLink = links.find((link) => link.textContent?.includes('Confirmed today'))!;
    expect(confirmedLink.getAttribute('href')).toBe('/admin/complex/complex-id/reservations');
    expect(within(confirmedLink).getByText('5')).toBeTruthy();

    const completedLink = links.find((link) => link.textContent?.includes('Completed today'))!;
    expect(completedLink.getAttribute('href')).toBe('/admin/complex/complex-id/reservations');
    expect(within(completedLink).getByText('3')).toBeTruthy();

    const cancelledLink = links.find((link) => link.textContent?.includes('Cancelled today'))!;
    expect(cancelledLink.getAttribute('href')).toBe('/admin/complex/complex-id/reservations');
    expect(within(cancelledLink).getByText('1')).toBeTruthy();

    const blockedLink = links.find((link) => link.textContent?.includes('Blocked users'))!;
    expect(blockedLink.getAttribute('href')).toBe('/admin/complex/complex-id/users');
    expect(within(blockedLink).getByText('2')).toBeTruthy();
  });

  it('renders loading skeletons while loading', () => {
    vi.mocked(useComplexDashboard).mockReturnValue(
      { data: null, isLoading: true, isError: false } as unknown as ReturnType<typeof useComplexDashboard>
    );

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId" element={<ComplexAdminPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-id' }
    );

    expect(document.querySelectorAll('.MuiSkeleton-root').length).toBeGreaterThan(0);
  });

  it('renders error state on failure', () => {
    vi.mocked(useComplexDashboard).mockReturnValue(
      { data: null, isLoading: false, isError: true } as unknown as ReturnType<typeof useComplexDashboard>
    );

    renderWithAuth(
      <Routes>
        <Route path="/admin/complex/:complexId" element={<ComplexAdminPage />} />
      </Routes>,
      { initialRoute: '/admin/complex/complex-id' }
    );

    expect(screen.getByText(/dashboard could not be loaded/i)).toBeTruthy();
  });
});
