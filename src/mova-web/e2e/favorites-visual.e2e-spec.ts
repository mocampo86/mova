import { test, type Page } from '@playwright/test';

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';
const SHOTS = 'test-results/favorites-shots';

function createFakeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.e2e-signature`;
}

function userToken(): string {
  return createFakeJwt({ sub: 'e2e-user', email: 'e2e@example.com', name: 'E2E User', roles: ['User'] });
}

function apiRoute(page: Page, method: string, pattern: RegExp, status: number, body: unknown) {
  return page.route(pattern, (route) => {
    if (route.request().method() !== method) {
      route.fallback();
      return;
    }
    return route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(body) });
  });
}

const complex = {
  id: 'complex-1',
  name: 'Rocha Courts',
  description: 'Canchas de Fútbol y Pádel con vestuarios.',
  address: 'Francisco de los Santos',
  city: 'Rocha',
  phoneNumber: '+598 99 000 000',
  email: 'rocha@test.com',
  status: 'Active',
  allowUserRecurringReservations: true,
  timeZoneId: 'America/Montevideo',
  currency: 'UYU',
  createdAt: '2026-10-01T00:00:00Z'
};

const favoriteItem = { ...complex, sports: ['Fútbol', 'Pádel'], createdAt: '2026-10-15T00:00:00Z' };

function emptyPaged(pageSize: number) {
  return { items: [], page: 1, pageSize, totalItems: 0, totalPages: 0 };
}

async function authenticate(page: Page) {
  await page.addInitScript((t) => {
    (window as unknown as Record<string, unknown>).__MOVA_E2E_TOKEN__ = t;
  }, userToken());
}

async function mockDashboard(page: Page) {
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/dashboard(\\?.*)?$`), 200, {
    user: { id: 'e2e-user', email: 'e2e@example.com', fullName: 'E2E User', phoneNumber: '+1234567890', phoneVerified: true },
    upcomingReservations: emptyPaged(5),
    historySummary: { totalItems: 0, recentReservations: [] },
    activeBlocks: []
  });
}

async function mockComplexesList(page: Page) {
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes(\\?.*)?$`), 200, {
    items: [complex, { ...complex, id: 'complex-2', name: 'Club Atlético', city: 'Montevideo' }],
    page: 1,
    pageSize: 12,
    totalItems: 2,
    totalPages: 1
  });
}

async function mockFavorites(page: Page, items: typeof favoriteItem[]) {
  const map = new Map(items.map((i) => [i.id, i]));

  await page.route(new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites/ids$`), (route) =>
    route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ items: [...map.keys()] }) })
  );
  await page.route(new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites(\\?.*)?$`), (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [...map.values()], page: 1, pageSize: 20, totalItems: map.size, totalPages: 1 })
    })
  );
  await page.route(new RegExp(`${API_BASE_URL}/api/v1/users/me/favorites/[0-9a-zA-Z-]+$`), (route) => {
    if (route.request().method() !== 'PUT' && route.request().method() !== 'DELETE') {
      return route.fallback();
    }
    return route.fulfill({ status: 204, body: '' });
  });
}

async function mockDetail(page: Page) {
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1(\\?.*)?$`), 200, complex);
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1/courts(\\?.*)?$`), 200, emptyPaged(100));
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/sports(\\?.*)?$`), 200, []);
  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/blocks/complex-1$`), 200, {
    isBlocked: false,
    complexId: 'complex-1',
    complexName: 'Rocha Courts'
  });
}

test.describe('favorites visual evidence', () => {
  test('captures all required screens', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 800 });
    await authenticate(page);
    await mockDashboard(page);
    await mockComplexesList(page);
    await mockFavorites(page, [favoriteItem]);
    await mockDetail(page);

    // Search with filled heart (complex-1 is favorite)
    await page.goto('/complexes');
    await page.getByRole('heading', { name: 'Rocha Courts' }).waitFor();
    await page.screenshot({ path: `${SHOTS}/search-desktop.png`, fullPage: false });

    // Favorites desktop
    await page.goto('/user/favorites');
    await page.getByRole('heading', { name: 'Rocha Courts' }).waitFor();
    await page.screenshot({ path: `${SHOTS}/favorites-desktop.png` });

    // Home with favorites
    await page.goto('/user');
    await page.locator('section[aria-labelledby="favorites-title"]').getByText('Rocha Courts').waitFor();
    await page.screenshot({ path: `${SHOTS}/home-desktop.png` });

    // Detail with active heart
    await page.goto('/complexes/complex-1');
    await page.getByRole('button', { name: 'Remove from favorites' }).waitFor();
    await page.screenshot({ path: `${SHOTS}/detail-desktop.png` });
  });

  test('captures mobile screens', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await authenticate(page);
    await mockDashboard(page);
    await mockComplexesList(page);
    await mockFavorites(page, [favoriteItem]);
    await mockDetail(page);

    await page.goto('/complexes');
    await page.getByRole('heading', { name: 'Rocha Courts' }).waitFor();
    await page.screenshot({ path: `${SHOTS}/search-mobile.png` });

    await page.goto('/user/favorites');
    await page.getByRole('heading', { name: 'Rocha Courts' }).waitFor();
    await page.screenshot({ path: `${SHOTS}/favorites-mobile.png` });
  });

  test('captures empty state and 320px nav', async ({ page }) => {
    await page.setViewportSize({ width: 320, height: 700 });
    await authenticate(page);
    await mockDashboard(page);
    await mockComplexesList(page);
    await mockFavorites(page, []);

    await page.goto('/user/favorites');
    await page.getByText("You don't have any favorite complexes yet.").waitFor();
    await page.screenshot({ path: `${SHOTS}/favorites-empty-320.png` });
  });
});
