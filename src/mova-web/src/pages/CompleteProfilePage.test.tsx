import { cleanup, fireEvent, screen, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import CompleteProfilePage from './CompleteProfilePage';
import { renderWithAuth } from '../test-utils';

describe('CompleteProfilePage', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    cleanup();
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          id: 'user-id',
          email: 'user@example.com',
          fullName: 'User',
          phoneNumber: '+54 11 1234 5678',
          phoneVerified: false
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

  it('renders the profile completion form', () => {
    renderWithAuth(<CompleteProfilePage />, {
      authState: {
        isAuthenticated: true,
        accessToken: 'test-token',
        user: { id: 'user-id', email: 'user@example.com', fullName: 'User', roles: ['User'] }
      }
    });

    expect(screen.getByText('Complete your profile')).toBeTruthy();
    expect(screen.getByLabelText('Phone number')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Continue' })).toBeTruthy();
  });

  it('shows validation error for an invalid phone number', async () => {
    renderWithAuth(<CompleteProfilePage />, {
      authState: {
        isAuthenticated: true,
        accessToken: 'test-token',
        user: { id: 'user-id', email: 'user@example.com', fullName: 'User', roles: ['User'] }
      }
    });

    const input = screen.getByLabelText('Phone number');
    fireEvent.change(input, { target: { value: '12345678' } });
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }));

    await waitFor(() => {
      expect(
        screen.getByText(/Phone number must be in international format/i)
      ).toBeTruthy();
    });
  });

  it('submits the phone number and navigates to /user on success', async () => {
    const completeProfile = vi.fn();

    renderWithAuth(<CompleteProfilePage />, {
      authState: {
        isAuthenticated: true,
        accessToken: 'test-token',
        user: { id: 'user-id', email: 'user@example.com', fullName: 'User', roles: ['User'] },
        completeProfile
      }
    });

    const input = screen.getByLabelText('Phone number');
    fireEvent.change(input, { target: { value: '+54 11 1234 5678' } });
    fireEvent.click(screen.getByRole('button', { name: 'Continue' }));

    await waitFor(() => {
      expect(fetchSpy).toHaveBeenCalledWith(
        'http://localhost:5000/api/v1/users/me',
        expect.objectContaining({
          method: 'PATCH',
          body: JSON.stringify({ phoneNumber: '+54 11 1234 5678' })
        })
      );
    });

    const callArgs = fetchSpy.mock.calls[0] as [string, RequestInit];
    const headers = callArgs[1].headers as Headers;
    expect(headers.get('Authorization')).toBe('Bearer test-token');

    await waitFor(() => {
      expect(completeProfile).toHaveBeenCalled();
      expect(window.location.pathname).toBe('/user');
    });
  });
});
