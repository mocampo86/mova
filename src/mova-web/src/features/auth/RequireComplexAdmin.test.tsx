import { cleanup, screen } from '@testing-library/react';
import { describe, it, expect, afterEach } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import { RequireComplexAdmin } from './RequireComplexAdmin';
import { renderWithAuth, type RenderWithAuthOptions } from '../../test-utils';

afterEach(cleanup);

function renderComplexAdminRoute(authState: RenderWithAuthOptions['authState'], initialRoute: string) {
  return renderWithAuth(
    <Routes>
      <Route path="login" element={<div>Login</div>} />
      <Route path="unauthorized" element={<div>Unauthorized</div>} />
      <Route
        path="admin/complex/:complexId"
        element={
          <RequireComplexAdmin>
            <div data-testid="protected">Protected</div>
          </RequireComplexAdmin>
        }
      />
    </Routes>,
    { initialRoute, authState }
  );
}

describe('RequireComplexAdmin', () => {
  it('redirects to login when the user is not authenticated', () => {
    renderComplexAdminRoute(undefined, '/admin/complex/complex-1');

    expect(window.location.pathname).toBe('/login');
    expect(screen.queryByTestId('protected')).toBeNull();
  });

  it('redirects to unauthorized when the user is not admin of the requested complex', () => {
    renderComplexAdminRoute(
      {
        isAuthenticated: true,
        user: {
          id: '1',
          email: 'admin@example.com',
          fullName: 'Admin',
          roles: ['ComplexAdmin'],
          complexes: [{ complexId: 'complex-2', role: 'ComplexAdmin' }]
        }
      },
      '/admin/complex/complex-1'
    );

    expect(window.location.pathname).toBe('/unauthorized');
    expect(screen.queryByTestId('protected')).toBeNull();
  });

  it('renders children when the user is admin of the requested complex', () => {
    renderComplexAdminRoute(
      {
        isAuthenticated: true,
        user: {
          id: '1',
          email: 'admin@example.com',
          fullName: 'Admin',
          roles: ['ComplexAdmin'],
          complexes: [{ complexId: 'complex-1', role: 'ComplexAdmin' }]
        }
      },
      '/admin/complex/complex-1'
    );

    expect(screen.getByTestId('protected')).toBeTruthy();
  });

  it('allows SuperAdmin to access any complex', () => {
    renderComplexAdminRoute(
      {
        isAuthenticated: true,
        user: {
          id: '1',
          email: 'super@example.com',
          fullName: 'Super Admin',
          roles: ['SuperAdmin']
        }
      },
      '/admin/complex/complex-1'
    );

    expect(screen.getByTestId('protected')).toBeTruthy();
  });
});
