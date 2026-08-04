import type { ReactNode } from 'react';
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
    login: vi.fn(),
    logout: vi.fn(),
    ...overrides
  };
}

export interface RenderWithAuthOptions {
  authState?: Partial<AuthState>;
  initialRoute?: string;
}

export function renderWithAuth(ui: ReactNode, { authState, initialRoute = '/' }: RenderWithAuthOptions = {}) {
  window.history.pushState({}, 'Test page', initialRoute);

  return render(
    <BrowserRouter>
      <AuthContext.Provider value={createMockAuthState(authState)}>{ui}</AuthContext.Provider>
    </BrowserRouter>
  );
}
