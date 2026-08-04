import { cleanup, screen } from '@testing-library/react';
import { describe, it, expect, afterEach } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import { RequireRole } from './RequireRole';
import { renderWithAuth, type RenderWithAuthOptions } from '../../test-utils';

afterEach(cleanup);

function renderRoleRoute(allowedRoles: Array<'User' | 'ComplexAdmin' | 'SuperAdmin'>, authState: RenderWithAuthOptions['authState'], initialRoute: string) {
  return renderWithAuth(
    <Routes>
      <Route path="login" element={<div>Login</div>} />
      <Route path="unauthorized" element={<div>Unauthorized</div>} />
      <Route
        path="admin/super"
        element={
          <RequireRole allowedRoles={allowedRoles}>
            <div data-testid="protected">Protected</div>
          </RequireRole>
        }
      />
    </Routes>,
    { initialRoute, authState }
  );
}

describe('RequireRole', () => {
  it('redirects to login when the user is not authenticated', () => {
    renderRoleRoute(['SuperAdmin'], undefined, '/admin/super');

    expect(window.location.pathname).toBe('/login');
    expect(screen.queryByTestId('protected')).toBeNull();
  });

  it('redirects to unauthorized when the user lacks the required role', () => {
    renderRoleRoute(
      ['SuperAdmin'],
      {
        isAuthenticated: true,
        user: {
          id: '1',
          email: 'user@example.com',
          fullName: 'User',
          roles: ['User']
        }
      },
      '/admin/super'
    );

    expect(window.location.pathname).toBe('/unauthorized');
    expect(screen.queryByTestId('protected')).toBeNull();
  });

  it('renders children when the user has the required role', () => {
    renderRoleRoute(
      ['SuperAdmin'],
      {
        isAuthenticated: true,
        user: {
          id: '1',
          email: 'admin@example.com',
          fullName: 'Admin',
          roles: ['SuperAdmin']
        }
      },
      '/admin/super'
    );

    expect(screen.getByTestId('protected')).toBeTruthy();
  });
});
