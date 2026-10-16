import { screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import UserLayout from './UserLayout';
import { renderWithAuth } from '../test-utils';

vi.mock('../features/users/useUserDashboard', () => ({ useUserDashboard: vi.fn() }));
vi.mock('./AppHeader', () => ({ default: () => <div>Desktop header</div> }));
import { useUserDashboard } from '../features/users/useUserDashboard';

function setMobileViewport() {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: (query: string) => ({
      matches: query.includes('max-width'), media: query, onchange: null,
      addListener: () => undefined, removeListener: () => undefined,
      addEventListener: () => undefined, removeEventListener: () => undefined,
      dispatchEvent: () => false
    })
  });
}

describe('UserLayout mobile navigation', () => {
  beforeEach(() => {
    setMobileViewport();
    vi.mocked(useUserDashboard).mockReturnValue({
      data: { user: { fullName: 'Alex', phoneNumber: '123' }, upcomingReservations: { items: [] } },
      isLoading: false, isError: false
    } as unknown as ReturnType<typeof useUserDashboard>);
  });

  it('renders all five persistent player destinations from the shared layout', () => {
    renderWithAuth(<Routes><Route element={<UserLayout />}><Route path="/user" element={<div>Home content</div>} /></Route></Routes>, { initialRoute: '/user' });
    const mobileNavigation = screen.getByRole('navigation', { name: 'Home' });
    expect(within(mobileNavigation).getByRole('link', { name: 'Home' }).getAttribute('href')).toBe('/user');
    expect(within(mobileNavigation).getByRole('link', { name: 'Search' }).getAttribute('href')).toBe('/complexes');
    expect(within(mobileNavigation).getByRole('link', { name: 'Favorites' }).getAttribute('href')).toBe('/user/favorites');
    expect(within(mobileNavigation).getByRole('link', { name: 'Bookings' }).getAttribute('href')).toBe('/user/reservations');
    expect(within(mobileNavigation).getByRole('link', { name: 'Profile' }).getAttribute('href')).toBe('/user/profile');
  });

  it('marks Favorites as active on /user/favorites', () => {
    renderWithAuth(<Routes><Route element={<UserLayout />}><Route path="/user/favorites" element={<div>Favorites content</div>} /></Route></Routes>, { initialRoute: '/user/favorites' });
    const mobileNavigation = screen.getByRole('navigation', { name: 'Home' });
    expect(within(mobileNavigation).getByRole('link', { name: 'Favorites' }).getAttribute('aria-current')).toBe('page');
  });

  it('shows My favorites in the desktop sidebar', () => {
    renderWithAuth(<Routes><Route element={<UserLayout />}><Route path="/user" element={<div>Home content</div>} /></Route></Routes>, { initialRoute: '/user' });
    expect(screen.getAllByRole('link', { name: 'My favorites' })[0].getAttribute('href')).toBe('/user/favorites');
  });
});
