import type { ReactNode } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { vi } from 'vitest';
import { AuthContext } from './features/auth/AuthContext';
import type { AuthState } from './features/auth/authTypes';

export function createMockAuthState(overrides: Partial<AuthState> = {}): AuthState {
  return {
    accessToken: null,
    user: null,
    isAuthenticated: false,
    requiresProfileCompletion: false,
    login: vi.fn(),
    logout: vi.fn(),
    completeProfile: vi.fn(),
    ...overrides
  };
}

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false
      },
      mutations: {
        retry: false
      }
    }
  });
}

export interface RenderWithAuthOptions {
  authState?: Partial<AuthState>;
  initialRoute?: string;
}

export function renderWithAuth(
  ui: ReactNode,
  { authState, initialRoute = '/' }: RenderWithAuthOptions = {}
) {
  window.history.pushState({}, 'Test page', initialRoute);

  return render(
    <QueryClientProvider client={createTestQueryClient()}>
      <BrowserRouter>
        <AuthContext.Provider value={createMockAuthState(authState)}>{ui}</AuthContext.Provider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
