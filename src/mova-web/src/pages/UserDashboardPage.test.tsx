import { screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import UserDashboardPage from './UserDashboardPage';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/users/useUserDashboard', () => ({ useUserDashboard: vi.fn() }));
vi.mock('../features/complexes/complexApi', () => ({ useActiveComplexes: vi.fn() }));

import { useUserDashboard } from '../features/users/useUserDashboard';
import { useActiveComplexes } from '../features/complexes/complexApi';

function setup(upcoming = true) {
  vi.mocked(useUserDashboard).mockReturnValue({
    data: {
      user: { fullName: 'Alex Player', email: 'alex@example.com' },
      upcomingReservations: { items: upcoming ? [{ id: 'reservation-1', complexId: 'complex-1', courtId: 'court-1', courtName: 'Court One', userId: 'user-1', userName: 'Alex', startAt: '2026-10-23T18:00:00Z', endAt: '2026-10-23T19:00:00Z', status: 'Confirmed', source: 'Web', createdAt: '2026-10-01T00:00:00Z' }] : [] }
    }, isLoading: false, isError: false
  } as unknown as ReturnType<typeof useUserDashboard>);
  vi.mocked(useActiveComplexes).mockReturnValue({
    data: { items: [{ id: 'complex-1', name: 'Central Padel', city: 'Montevideo', address: 'Main St', description: 'Padel courts' }] }, isLoading: false, isError: false
  } as unknown as ReturnType<typeof useActiveComplexes>);
}

describe('UserDashboardPage', () => {
  it('prioritizes the compact greeting, court search and next real reservation', () => {
    setup();
    renderWithAuth(<UserDashboardPage />, { initialRoute: '/user' });
    expect(screen.getByRole('heading', { name: 'Hi, Alex Player!' })).toBeTruthy();
    expect(screen.getByText('Where are we playing today?')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Browse complexes' }).getAttribute('href')).toBe('/complexes');
    expect(screen.getByText('Court One')).toBeTruthy();
    expect(screen.getAllByText('Central Padel').length).toBeGreaterThan(0);
  });

  it('renders a useful compact empty state without upcoming reservations', () => {
    setup(false);
    renderWithAuth(<UserDashboardPage />, { initialRoute: '/user' });
    expect(screen.getByText('You have no upcoming reservations.')).toBeTruthy();
    expect(screen.getByText('Explore nearby sports complexes and choose a court that works for you.')).toBeTruthy();
  });
});
