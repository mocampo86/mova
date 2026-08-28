import { cleanup, screen } from '@testing-library/react';
import { afterEach, describe, expect, it } from 'vitest';
import { Route, Routes } from 'react-router-dom';
import AuthLayout from './AuthLayout';
import { renderWithAuth } from '../test-utils';

function renderLayout(initialRoute: string) {
  return renderWithAuth(
    <Routes>
      <Route element={<AuthLayout />}>
        <Route
          path="complete-profile"
          element={<div data-testid="auth-child">Auth child</div>}
        />
      </Route>
    </Routes>,
    { initialRoute }
  );
}

describe('AuthLayout', () => {
  afterEach(cleanup);

  it('renders the app header and the matched route outlet', () => {
    renderLayout('/complete-profile');

    expect(screen.getByTestId('auth-child')).toBeTruthy();
    expect(screen.getByRole('banner')).toBeTruthy();
    expect(screen.getByRole('link', { name: /MOVA/i })).toBeTruthy();
  });
});
