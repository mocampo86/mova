import type { ReactNode } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { AuthContext } from './AuthContext';
import type { AuthState } from './authTypes';
import { useGoogleLogin } from './useGoogleLogin';

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate
  };
});

const mockAuthState: AuthState = {
  accessToken: null,
  user: null,
  isAuthenticated: false,
  requiresProfileCompletion: false,
  login: vi.fn(),
  logout: vi.fn(),
  completeProfile: vi.fn()
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false }
    }
  });

  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AuthContext.Provider value={mockAuthState}>{children}</AuthContext.Provider>
        </BrowserRouter>
      </QueryClientProvider>
    );
  };
}

describe('useGoogleLogin', () => {
  let fetchSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          accessToken: 'test-token',
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
    mockNavigate.mockClear();
  });

  it('redirects to /user after a successful user login with a complete profile', async () => {
    const { result } = renderHook(() => useGoogleLogin('user'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync({ idToken: 'google-id-token' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockNavigate).toHaveBeenCalledWith('/user', { replace: true });
  });

  it('redirects to /complete-profile when the profile requires completion', async () => {
    fetchSpy.mockResolvedValue(
      new Response(
        JSON.stringify({
          accessToken: 'test-token',
          requiresProfileCompletion: true
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' }
        }
      )
    );

    const { result } = renderHook(() => useGoogleLogin('user'), {
      wrapper: createWrapper()
    });

    await waitFor(() => expect(result.current.mutateAsync).toBeTruthy());

    await result.current.mutateAsync({ idToken: 'google-id-token' });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockNavigate).toHaveBeenCalledWith('/complete-profile', { replace: true });
  });
});
