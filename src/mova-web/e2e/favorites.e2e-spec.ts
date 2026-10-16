import { test, expect, type Page } from '@playwright/test';

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';

function createFakeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.e2e-signature`;
}

function userToken(): string {
  return createFakeJwt({
    sub: 'e2e-user',
    email: 'e2e@example.com',
    name: 'E2E User',
    roles: ['User']
  });
}

function apiRoute(page: Page, method: string, pattern: RegExp, status: number, body: unknown) {
  return page.route(pattern, (route) => {
    if (route.request().method() !== method) {
      route.continue();
      return;
    }

    return route.fulfill({
      status,
      contentType: 'application/json',
      body: JSON.stringify(body)
    });
  });
}

async function authenticate(page: Page, token: string) {
  await page.addInitScript((t) => {
    (window as unknown as Record<string, unknown>).__MOVA_E2E_TOKEN__ = t;
  }, token);
}

const complex = {
  id: 'complex-1',
  name: 'Rocha Courts',
  description: 'Padel club',
  address: 'Ruta 9 km 210',
  city: 'Rocha',
  phoneNumber: '+598 99 000 000',
  email: 'rocha@test.com',
  status: 'Active',
  allowUserRecurringReservations: true,
  timeZoneId: 'America/Montevideo',
  currency: 'UYU',
  createdAt: '2026-10-01T00:00:00Z'
};

const favoriteComplex = {
  ...complex,
  sports: ['Padel'],
  createdAt: '2026-10-15T00:00:00Z'
};

function emptyPaged(pageSize: number) {
  return { items: [], page: 1, pageSize, totalItems: 0, totalPages: 0 };
}

async function mockDashboard(page: Page) {
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/dashboard(\\?.*)?$`), 200, {
    user: {
      id: 'e2e-user',
      email: 'e2e@example.com',
      fullName: 'E2E User',
      phoneNumber: '+1234567890',
      phoneVerified: true
    },
    upcomingReservations: emptyPaged(5),
    historySummary: { totalItems: 0, recentReservations: [] },
    activeBlocks: []
  });
}

async function mockComplexesList(page: Page) {
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes(\\?.*)?$`), 200, {
    items: [complex],
    page: 1,
    pageSize: 12,
    totalItems: 1,
    totalPages: 1
  });
}

async function mockFavoritesState(page: Page) {
  const favorites = new Map<string, typeof favoriteComplex>();

  await page.route(new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites/ids$`), (route) => {
    if (route.request().method() !== 'GET') {
      route.fallback();
      return;
    }
    return route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [...favorites.keys()] })
    });
  });

  await page.route(new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites(\\?.*)?$`), (route) => {
    if (route.request().method() !== 'GET') {
      route.fallback();
      return;
    }
    const items = [...favorites.values()];
    return route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items, page: 1, pageSize: 20, totalItems: items.length, totalPages: 1 })
    });
  });

  await page.route(new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites/[0-9a-zA-Z-]+$`), (route) => {
    const method = route.request().method();
    const complexId = route.request().url().split('/').pop()!;
    if (method === 'PUT') {
      favorites.set(complexId, { ...favoriteComplex, id: complexId });
      return route.fulfill({ status: 204, body: '' });
    }
    if (method === 'DELETE') {
      favorites.delete(complexId);
      return route.fulfill({ status: 204, body: '' });
    }
    return route.fallback();
  });
}

test.describe('User favorites', () => {
  test('adds a favorite from search and sees it on the favorites page and dashboard', async ({ page }) => {
    await authenticate(page, userToken());
    await mockDashboard(page);
    await mockComplexesList(page);
    await mockFavoritesState(page);

    await page.goto('/complexes');
    await page.getByRole('heading', { name: 'Rocha Courts' }).waitFor();

    await page.getByRole('button', { name: 'Add to favorites' }).click();
    await expect(page.getByText('Added to favorites')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Remove from favorites' })).toBeVisible();

    await page.goto('/user/favorites');
    await expect(page.getByRole('heading', { name: 'My favorites', level: 1 })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Rocha Courts' })).toBeVisible();
    await expect(page.getByText('Padel')).toBeVisible();

    await page.getByRole('link', { name: 'View courts and hours' }).click();
    await expect(page).toHaveURL(/\/complexes\/complex-1/);

    await page.goto('/user');
    const favoritesSection = page.locator('section[aria-labelledby="favorites-title"]');
    await expect(favoritesSection.getByText('Rocha Courts')).toBeVisible();
  });

  test('removes a favorite from the favorites page', async ({ page }) => {
    await authenticate(page, userToken());
    await mockDashboard(page);
    await mockComplexesList(page);
    await mockFavoritesState(page);

    await page.goto('/complexes');
    await page.getByRole('button', { name: 'Add to favorites' }).click();
    await expect(page.getByText('Added to favorites')).toBeVisible();

    await page.goto('/user/favorites');
    await page.getByRole('button', { name: 'Remove from favorites' }).click();
    await expect(page.getByText("You don't have any favorite complexes yet.")).toBeVisible();
    await expect(page.getByRole('link', { name: 'Explore complexes' })).toBeVisible();
  });

  test('shows five mobile destinations with Favorites active on the favorites page', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await authenticate(page, userToken());
    await mockDashboard(page);
    await mockComplexesList(page);
    await mockFavoritesState(page);

    await page.goto('/user/favorites');

    const nav = page.locator('nav').last();
    await expect(nav.getByRole('link', { name: 'Home' })).toBeVisible();
    await expect(nav.getByRole('link', { name: 'Search' })).toBeVisible();
    const favoritesLink = nav.getByRole('link', { name: 'Favorites' });
    await expect(favoritesLink).toBeVisible();
    await expect(nav.getByRole('link', { name: 'Bookings' })).toBeVisible();
    await expect(nav.getByRole('link', { name: 'Profile' })).toBeVisible();
    await expect(favoritesLink).toHaveAttribute('aria-current', 'page');

    const bodyWidth = await page.evaluate(() => document.body.scrollWidth);
    expect(bodyWidth).toBeLessThanOrEqual(375);
  });

  test('redirects anonymous visitors to login with the return destination', async ({ page }) => {
    await mockComplexesList(page);
    await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites/ids$`), 401, {});

    await page.goto('/complexes');
    await page.getByRole('heading', { name: 'Rocha Courts' }).waitFor();
    await page.getByRole('button', { name: 'Add to favorites' }).click();

    await expect(page).toHaveURL(/\/login\?intent=user&returnTo=/);
  });

  test('shows an error state when the favorites API fails', async ({ page }) => {
    await authenticate(page, userToken());
    await mockDashboard(page);
    await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites(\\?.*)?$`), 500, {
      error: { code: 'Error', message: 'Boom' }
    });
    await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites/ids$`), 200, { items: [] });
    await mockComplexesList(page);

    await page.goto('/user/favorites');
    await expect(page.getByText("We couldn't load your favorites. Please try again.")).toBeVisible({ timeout: 20000 });
    await expect(page.getByRole('button', { name: 'Try again' })).toBeVisible();
  });

  test('keeps favorites state synchronized between search and detail', async ({ page }) => {
    await authenticate(page, userToken());
    await mockDashboard(page);
    await mockComplexesList(page);
    await mockFavoritesState(page);
    await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1(\\?.*)?$`), 200, complex);
    await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1/courts(\\?.*)?$`), 200, emptyPaged(100));
    await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/sports(\\?.*)?$`), 200, []);
    await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/blocks/complex-1$`), 200, {
      isBlocked: false,
      complexId: 'complex-1',
      complexName: 'Rocha Courts'
    });

    await page.goto('/complexes');
    await page.getByRole('button', { name: 'Add to favorites' }).click();
    await expect(page.getByText('Added to favorites')).toBeVisible();

    await page.getByRole('link', { name: 'View courts and times' }).click();
    await expect(page.getByRole('button', { name: 'Remove from favorites' })).toBeVisible();

    await page.getByRole('button', { name: 'Remove from favorites' }).click();
    await page.goto('/complexes');
    await page.getByRole('heading', { name: 'Rocha Courts' }).waitFor();
    await expect(page.getByRole('button', { name: 'Add to favorites' })).toBeVisible();
  });
});
