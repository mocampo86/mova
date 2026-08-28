import { screen, fireEvent } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
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

describe('AppHeader', () => {
  it('displays public login and admin access when the visitor is not authenticated', () => {
    renderWithAuthState();

    const userLogin = screen.getByRole('link', { name: 'User login' });
    expect(userLogin.getAttribute('href')).toBe('/login?intent=user');

    const adminLogin = screen.getByRole('link', { name: 'Admin login' });
    expect(adminLogin.getAttribute('href')).toBe('/login?intent=complex');
  });

  it('displays the user dashboard and logout when the visitor is authenticated', () => {
    const logout = vi.fn();
    renderWithAuthState({
      isAuthenticated: true,
      user: mockUser,
      accessToken: 'test-token',
      logout
    });

    expect(screen.getByRole('link', { name: 'Dashboard' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Log out' })).toBeTruthy();
  });

  it('calls logout when the visitor clicks the logout button', () => {
    const logout = vi.fn();
    renderWithAuthState({
      isAuthenticated: true,
      user: mockUser,
      accessToken: 'test-token',
      logout
    });

    fireEvent.click(screen.getByRole('button', { name: 'Log out' }));
    expect(logout).toHaveBeenCalled();
  });
});
