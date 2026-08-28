import { test, expect } from '@playwright/test';

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';

test.describe('US-053 Basic SEO metadata on public pages', () => {
  test.setTimeout(60000);

  test('home page exposes title, meta description, and Open Graph tags', async ({ page }) => {
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

    await expect(page).toHaveTitle('Mova | Find your next game');
    await expect(page.locator('meta[name="description"]')).toHaveAttribute(
      'content',
      'Discover nearby sports complexes, check court availability, and reserve the time that works for you.'
    );
    await expect(page.locator('meta[property="og:title"]')).toHaveAttribute('content', 'Mova | Find your next game');
    await expect(page.locator('meta[property="og:description"]')).toHaveAttribute(
      'content',
      'Discover nearby sports complexes, check court availability, and reserve the time that works for you.'
    );
    await expect(page.locator('meta[property="og:type"]')).toHaveAttribute('content', 'website');
  });

  test('login page exposes a title and meta description', async ({ page }) => {
    await page.goto('/login?intent=user');

    await expect(page).toHaveTitle('Mova | Sign in to play');
    await expect(page.locator('meta[name="description"]')).toHaveAttribute(
      'content',
      'Sign in with your Google account to continue.'
    );
  });

  test('not found page exposes a title and meta description', async ({ page }) => {
    await page.goto('/not-a-real-page');

    await expect(page).toHaveTitle('Mova | 404 - Page not found');
    await expect(page.locator('meta[name="description"]')).toHaveAttribute(
      'content',
      'The page you are looking for does not exist.'
    );
  });
});
