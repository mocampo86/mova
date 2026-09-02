import { test, expect } from '@playwright/test';

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';

async function mockComplexes(page: import('@playwright/test').Page) {
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
}

async function assertNoHorizontalOverflow(page: import('@playwright/test').Page) {
  const { scrollWidth, clientWidth } = await page.evaluate(() => ({
    scrollWidth: document.documentElement.scrollWidth,
    clientWidth: document.documentElement.clientWidth
  }));
  expect(scrollWidth, 'page should not overflow horizontally').toBeLessThanOrEqual(clientWidth + 1);
}

test.describe('US-14.4 Implement Responsive Design', () => {
  test.setTimeout(60000);

  test('public landing is usable at a desktop viewport', async ({ page }) => {
    await mockComplexes(page);
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await expect(page.getByRole('heading', { name: 'Find your next game.' })).toBeVisible();

    const hero = page.getByTestId('hero');
    await expect(hero.getByRole('link', { name: 'Play / Book a court' })).toBeVisible();
    await expect(hero.getByRole('link', { name: 'Manage your complex' })).toBeVisible();
    await expect(hero.getByRole('link', { name: 'Browse complexes' })).toBeVisible();

    await expect(page.getByRole('link', { name: 'Sign in to play' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Register your complex' })).toBeVisible();

    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await page.waitForTimeout(250);

    await assertNoHorizontalOverflow(page);
  });

  test('public landing is usable at a mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await mockComplexes(page);
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    await expect(page.getByRole('heading', { name: 'Find your next game.' })).toBeVisible();

    const hero = page.getByTestId('hero');
    await expect(hero.getByRole('link', { name: 'Play / Book a court' })).toBeVisible();
    await expect(hero.getByRole('link', { name: 'Manage your complex' })).toBeVisible();
    await expect(hero.getByRole('link', { name: 'Browse complexes' })).toBeVisible();

    const menuButton = page.getByRole('button', { name: 'open menu' });
    await expect(menuButton).toBeVisible();
    await menuButton.click();

    await expect(page.getByRole('link', { name: 'User login' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Admin login' })).toBeVisible();
    await expect(page.getByRole('combobox', { name: 'Language' })).toBeVisible();

    await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await page.waitForTimeout(250);

    await assertNoHorizontalOverflow(page);
  });
});
