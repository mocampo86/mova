import { test, expect, type Locator, type Page } from '@playwright/test';

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';

function createFakeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  return `${header}.${body}.e2e-signature`;
}

function userToken(roles: string[] = ['User']): string {
  return createFakeJwt({
    sub: 'e2e-user',
    email: 'e2e@example.com',
    name: 'E2E User',
    roles
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

async function setupRecurringMocks(page: Page) {
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

  const today = new Date();
  const startDate = today.toISOString().split('T')[0];
  const nextWeek = new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000);
  const endDate = nextWeek.toISOString().split('T')[0];

  const startAt = `${startDate}T18:00:00Z`;
  const endAt = `${startDate}T19:00:00Z`;

  const createdRecurring = {
    id: 'recurring-1',
    complexId: 'complex-1',
    courtId: 'court-1',
    userId: 'e2e-user',
    dayOfWeek: 1,
    startTime: '18:00:00',
    durationMinutes: 60,
    startDate,
    endDate,
    status: 'Active',
    createdAt: new Date().toISOString(),
    occurrences: [
      {
        id: 'occurrence-1',
        complexId: 'complex-1',
        courtId: 'court-1',
        userId: 'e2e-user',
        startAt,
        endAt,
        status: 'Confirmed',
        source: 'Recurring',
        recurringReservationId: 'recurring-1',
        createdAt: new Date().toISOString()
      }
    ]
  };

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/users/me/dashboard(\\?.*)?$`), 200, {
    user: {
      id: 'e2e-user',
      email: 'e2e@example.com',
      fullName: 'E2E User',
      phoneNumber: '+1234567890',
      phoneVerified: true
    },
    upcomingReservations: { items: [], page: 1, pageSize: 5, totalItems: 0, totalPages: 0 },
    historySummary: { totalItems: 0, recentReservations: [] },
    activeBlocks: []
  });

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes(\\?.*)?$`), 200, {
    items: [complex],
    page: 1,
    pageSize: 12,
    totalItems: 1,
    totalPages: 1
  });

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1(\\?.*)?$`), 200, complex);

  await apiRoute(page, 'GET', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1/courts(\\?.*)?$`), 200, {
    items: [court],
    page: 1,
    pageSize: 100,
    totalItems: 1,
    totalPages: 1
  });

  await apiRoute(page, 'POST', new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1/recurring-reservations/me`), 201, createdRecurring);
}

async function authenticate(page: Page, token: string) {
  await page.addInitScript((t) => {
    (window as unknown as Record<string, unknown>).__MOVA_E2E_TOKEN__ = t;
  }, token);
}

async function selectMuiValue(page: Page, selectLocator: Locator, value: string) {
  await selectLocator.click();
  const option = page.locator(`li[role="option"][data-value="${value}"]`);
  await expect(option).toBeVisible();
  await option.click();
  await expect(option).not.toBeVisible();
}

test.describe('EPIC-07 Recurring reservations', () => {
  test.setTimeout(60000);

  test('a user can create a weekly recurring reservation for an active complex', async ({ page }) => {
    await authenticate(page, userToken());
    await setupRecurringMocks(page);

    await page.goto('/user/recurring');

    await expect(page.getByRole('heading', { name: 'Recurring bookings' })).toBeVisible();

    const complexSelect = page.getByTestId('recurring-complex-select');
    await expect(complexSelect).toBeEnabled();
    await selectMuiValue(page, complexSelect, 'complex-1');

    const courtSelect = page.getByTestId('recurring-court-select');
    await expect(courtSelect).toBeEnabled({ timeout: 30000 });
    await selectMuiValue(page, courtSelect, 'court-1');

    const daySelect = page.getByTestId('recurring-day-select');
    await selectMuiValue(page, daySelect, '1');

    await expect(page.getByLabel('Start time')).toBeEnabled({ timeout: 30000 });

    const today = new Date();
    const startDate = today.toISOString().split('T')[0];
    const nextWeek = new Date(today.getTime() + 7 * 24 * 60 * 60 * 1000);
    const endDate = nextWeek.toISOString().split('T')[0];

    await page.getByLabel('Start time').fill('18:00');
    await page.getByLabel('Duration (minutes)').fill('60');
    await page.getByLabel('Start date').fill(startDate);
    await page.getByLabel('End date').fill(endDate);

    await expect(page.getByText(/This will create \d+ weekly occurrences/i)).toBeVisible();

    await page.getByRole('button', { name: 'Create recurring booking' }).click();

    await expect(page.getByText('Recurring booking created with 1 confirmed occurrences.')).toBeVisible();
  });
});
