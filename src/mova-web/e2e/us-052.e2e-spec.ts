import { test, expect } from '@playwright/test';

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';

test.describe('US-052 Access login and registration from the landing page', () => {
  test.setTimeout(60000);

  test('landing page exposes login and registration links and reaches the login page', async ({ page }) => {
    await page.route(new RegExp(`${API_BASE_URL}/api/v1/complexes(\\?.*)?$`), (route) => {
      if (route.request().method() !== 'GET') {
        route.continue();
        return;
      }

      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [],
          page: 1,
          pageSize: 12,
          totalItems: 0,
          totalPages: 0
        })
      });
    });

    await page.goto('/');

    await expect(page.getByRole('heading', { name: 'Find your next game.' })).toBeVisible();

    const playerLogin = page.getByRole('link', { name: 'Sign in to play' });
    await expect(playerLogin).toBeVisible();
    await expect(playerLogin).toHaveAttribute('href', '/login?intent=user');

    const ownerRegister = page.getByRole('link', { name: 'Register your complex' });
    await expect(ownerRegister).toBeVisible();
    await expect(ownerRegister).toHaveAttribute('href', '/login?intent=complex');

    await playerLogin.click();
    await expect(page).toHaveURL('/login?intent=user');
    await expect(page.getByRole('heading', { name: 'Welcome to Mova' })).toBeVisible();
  });
});
