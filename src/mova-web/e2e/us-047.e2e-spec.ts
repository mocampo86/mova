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

async function setupBlockStatusMocks(page: Page, blockStatus: unknown) {
  const complex = {
    id: 'complex-1',
    name: 'E2E Complex',
    description: 'Test complex',
    address: 'Test address',
    city: 'Test city',
    phoneNumber: '+1234567890',
    email: 'complex@test.com',
    status: 'Active',
    allowUserRecurringReservations: true,
    timeZoneId: 'America/Montevideo'
  };

  const court = {
    id: 'court-1',
    sportsComplexId: 'complex-1',
    name: 'E2E Court',
    description: 'Test court',
    surfaceType: 'Concrete',
    indoor: false,
    status: 'Active',
    sportIds: []
  };

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1(\\?.*)?$`), 200, complex);

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1/courts(\\?.*)?$`), 200, {
    items: [court],
    page: 1,
    pageSize: 100,
    totalItems: 1,
    totalPages: 1
  });

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/sports$`), 200, []);

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/blocks/complex-1$`), 200, blockStatus);
}

test.describe('US-047 See if I am blocked in a complex', () => {
  test.setTimeout(60000);

  test('shows a block warning when the user is blocked in the complex', async ({ page }) => {
    await authenticate(page, userToken());
    await setupBlockStatusMocks(page, {
      isBlocked: true,
      complexId: 'complex-1',
      complexName: 'E2E Complex',
      reason: 'No-show',
      blockedAt: new Date().toISOString()
    });

    await page.goto('/complexes/complex-1');

    await expect(page.getByText('You are blocked in E2E Complex: No-show.')).toBeVisible();
  });

  test('shows the block expiration when the block has a blockedUntil date', async ({ page }) => {
    await authenticate(page, userToken());
    const blockedUntil = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString();
    await setupBlockStatusMocks(page, {
      isBlocked: true,
      complexId: 'complex-1',
      complexName: 'E2E Complex',
      reason: 'No-show',
      blockedAt: new Date().toISOString(),
      blockedUntil
    });

    await page.goto('/complexes/complex-1');

    await expect(page.getByText(/You are blocked in E2E Complex: No-show/i)).toBeVisible();
    await expect(page.getByText(/Expires on/i)).toBeVisible();
  });

  test('does not show a block warning when the user is not blocked', async ({ page }) => {
    await authenticate(page, userToken());
    await setupBlockStatusMocks(page, {
      isBlocked: false,
      complexId: 'complex-1',
      complexName: 'E2E Complex'
    });

    await page.goto('/complexes/complex-1');

    await expect(page.getByText(/You are blocked/i)).not.toBeVisible();
  });
});
