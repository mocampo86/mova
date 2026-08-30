import { cleanup, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import AppHeader from './AppHeader';
import { renderWithAuth } from '../test-utils';
import type { AuthState, UserRole } from '../features/auth/authTypes';

const renderWithAuthState = (authState: Partial<AuthState> = {}) =>
  renderWithAuth(<AppHeader />, { authState });

const mockUser = {
  id: 'user-1',
  email: 'user@example.com',
  fullName: 'John Doe',
  roles: ['User' as UserRole]
};

function mockMatchMedia(matches: boolean) {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches,
      media: query,
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn()
    }))
  });
}

describe('AppHeader', () => {
  beforeEach(() => {
    mockMatchMedia(false);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    cleanup();
  });

  it('displays the brand linked to the homepage', () => {
    renderWithAuthState();

    const brand = screen.getByRole('link', { name: 'Mova' });
    expect(brand.getAttribute('href')).toBe('/');
  });

  it('marks the brand as the current page when on the homepage', () => {
    renderWithAuth(<AppHeader />, { initialRoute: '/' });

    const brand = screen.getByRole('link', { name: 'Mova' });
    expect(brand.getAttribute('aria-current')).toBe('page');
  });

  it('displays public login and admin access when the visitor is not authenticated', () => {
    renderWithAuthState();

    const userLogin = screen.getByRole('link', { name: 'User login' });
    expect(userLogin.getAttribute('href')).toBe('/login?intent=user');

    const adminLogin = screen.getByRole('link', { name: 'Admin login' });
    expect(adminLogin.getAttribute('href')).toBe('/login?intent=complex');
  });

  it('preserves login intent query parameters on public navigation', () => {
    renderWithAuth(<AppHeader />, { initialRoute: '/complexes' });

    expect(screen.getByRole('link', { name: 'User login' }).getAttribute('href')).toBe('/login?intent=user');
    expect(screen.getByRole('link', { name: 'Admin login' }).getAttribute('href')).toBe('/login?intent=complex');
  });

  it('marks the active login intent on the login page', () => {
    renderWithAuth(<AppHeader />, { initialRoute: '/login?intent=user' });

    const userLogin = screen.getByRole('link', { name: 'User login' });
    expect(userLogin.getAttribute('aria-current')).toBe('page');

    const adminLogin = screen.getByRole('link', { name: 'Admin login' });
    expect(adminLogin.getAttribute('aria-current')).not.toBe('page');
  });

  it('displays the user dashboard and logout when the visitor is authenticated', () => {
    renderWithAuthState({
      isAuthenticated: true,
      user: mockUser,
      accessToken: 'test-token',
      logout: vi.fn()
    });

    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Log out' })).toBeTruthy();
  });

  it('marks the dashboard link as current on user pages', () => {
    renderWithAuth(<AppHeader />, {
      authState: { isAuthenticated: true, user: mockUser, accessToken: 'test-token' },
      initialRoute: '/user/reservations'
    });

    const dashboard = screen.getByRole('link', { name: 'Dashboard' });
    expect(dashboard.getAttribute('aria-current')).toBe('page');
  });

  it('calls logout when the visitor clicks the logout button', async () => {
    const logout = vi.fn();
    renderWithAuthState({
      isAuthenticated: true,
      user: mockUser,
      accessToken: 'test-token',
      logout
    });

    const logoutButton = screen.getByRole('button', { name: 'Log out' });
    await userEvent.click(logoutButton);
    expect(logout).toHaveBeenCalled();
  });

  it('keeps the user layout menu toggle on narrow screens', async () => {
    mockMatchMedia(true);
    const onMenuToggle = vi.fn();
    renderWithAuth(
      <AppHeader showMenuToggle onMenuToggle={onMenuToggle} />,
      {
        authState: { isAuthenticated: true, user: mockUser, accessToken: 'test-token' }
      }
    );

    const menuButton = screen.getByRole('button', { name: 'open menu' });
    await userEvent.click(menuButton);
    expect(onMenuToggle).toHaveBeenCalled();
  });

  it('collapses visitor actions into a mobile menu on narrow screens', async () => {
    mockMatchMedia(true);
    renderWithAuth(<AppHeader />, { initialRoute: '/' });

    const menuButton = screen.getByRole('button', { name: 'open menu' });
    await userEvent.click(menuButton);

    await waitFor(() => {
      expect(screen.getByRole('link', { name: 'User login' })).toBeTruthy();
      expect(screen.getByRole('link', { name: 'Admin login' })).toBeTruthy();
      expect(screen.getByRole('combobox', { name: 'Language' })).toBeTruthy();
    });

    const userLogin = screen.getByRole('link', { name: 'User login' });
    expect(userLogin.getAttribute('href')).toBe('/login?intent=user');
  });

  it('collapses authenticated actions into a mobile menu on narrow screens', async () => {
    mockMatchMedia(true);
    renderWithAuth(<AppHeader />, {
      authState: { isAuthenticated: true, user: mockUser, accessToken: 'test-token' },
      initialRoute: '/admin/super'
    });

    const menuButton = screen.getByRole('button', { name: 'open menu' });
    await userEvent.click(menuButton);

    await waitFor(() => {
      expect(screen.getByRole('link', { name: 'Dashboard' })).toBeTruthy();
      expect(screen.getByRole('button', { name: 'Log out' })).toBeTruthy();
      expect(screen.getByRole('combobox', { name: 'Language' })).toBeTruthy();
    });

    expect(screen.getByRole('link', { name: 'Dashboard' }).getAttribute('href')).toBe('/user');
  });
});
