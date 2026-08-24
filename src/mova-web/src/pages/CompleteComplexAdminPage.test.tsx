import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import CompleteComplexAdminPage from './CompleteComplexAdminPage';
import { renderWithAuth } from '../test-utils';

describe('CompleteComplexAdminPage', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    cleanup();
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          accessToken: 'new-test-token',
          expiresAt: new Date().toISOString(),
          user: {
            id: 'user-id',
            email: 'admin@example.com',
            fullName: 'Admin User',
            phoneNumber: '+54 11 1234 5678',
            phoneVerified: false
          },
          complexId: 'complex-id',
          requiresProfileCompletion: false
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );
  });

  afterEach(() => {
    fetchSpy.mockRestore();
    cleanup();
  });

  it('renders the complex admin profile completion form', () => {
    renderWithAuth(<CompleteComplexAdminPage />, {
      authState: {
        isAuthenticated: true,
        accessToken: 'test-token',
        user: { id: 'user-id', email: 'admin@example.com', fullName: 'Admin User', roles: ['User'] }
      }
    });

    expect(screen.getByText('Complete your complex profile')).toBeTruthy();
    expect(screen.getByLabelText('Your phone number')).toBeTruthy();
    expect(screen.getByLabelText('Complex name')).toBeTruthy();
    expect(screen.getByLabelText('Description')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Submit for review' })).toBeTruthy();
  });

  it('shows validation error for an invalid phone number', async () => {
    renderWithAuth(<CompleteComplexAdminPage />, {
      authState: {
        isAuthenticated: true,
        accessToken: 'test-token',
        user: { id: 'user-id', email: 'admin@example.com', fullName: 'Admin User', roles: ['User'] }
      }
    });

    const phoneInput = screen.getByLabelText('Your phone number');
    fireEvent.change(phoneInput, { target: { value: '12345678' } });
    fireEvent.click(screen.getByRole('button', { name: 'Submit for review' }));

    await waitFor(() => {
      expect(
        screen.getByText(/Phone number must be in international format/i)
      ).toBeTruthy();
    });
  });

  it('shows all required complex information fields', () => {
    renderWithAuth(<CompleteComplexAdminPage />, {
      authState: {
        isAuthenticated: true,
        accessToken: 'test-token',
        user: { id: 'user-id', email: 'admin@example.com', fullName: 'Admin User', roles: ['User'] }
      }
    });

    expect(screen.getByLabelText('Your phone number')).toBeTruthy();
    expect(screen.getByLabelText('Complex name')).toBeTruthy();
    expect(screen.getByLabelText('Description')).toBeTruthy();
    expect(screen.getByLabelText('Address')).toBeTruthy();
    expect(screen.getByLabelText('City')).toBeTruthy();
    expect(screen.getByLabelText('Complex phone number')).toBeTruthy();
    expect(screen.getByLabelText('Complex email')).toBeTruthy();
    expect(screen.getByLabelText('Latitude (optional)')).toBeTruthy();
    expect(screen.getByLabelText('Longitude (optional)')).toBeTruthy();
    expect(screen.getByLabelText(/Time zone/i)).toBeTruthy();
  });
});
