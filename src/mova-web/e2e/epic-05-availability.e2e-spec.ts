import { test, expect, type APIRequestContext, type Page } from '@playwright/test';

interface ActiveComplex {
  id: string;
  name: string;
  timeZoneId: string;
}

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';

async function getFirstActiveComplex(request: APIRequestContext): Promise<ActiveComplex | null> {
  const response = await request.get(`${API_BASE_URL}/api/v1/complexes?page=1&pageSize=1`);
  if (!response.ok()) return null;
  const body = await response.json();
  const first = body.items?.[0];
  if (!first || !first.id) return null;
  const detail = await request.get(`${API_BASE_URL}/api/v1/complexes/${first.id}`);
  if (!detail.ok()) return null;
  return await detail.json();
}

async function expectedTodayInTimeZone(page: Page, timeZoneId: string): Promise<string> {
  return await page.evaluate(
    ({ timeZoneId }) => new Date().toLocaleDateString('en-CA', { timeZone: timeZoneId }),
    { timeZoneId }
  );
}

test.describe('EPIC-05 Public Availability and Discovery', () => {
  test('landing page loads with discovery call-to-action', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByRole('heading', { name: 'Find your next game.' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Browse complexes' })).toBeVisible();
  });

  test('public complex list can be searched and navigated', async ({ page }) => {
    await page.goto('/complexes');

    await expect(page.getByRole('heading', { name: 'Find a sports complex' })).toBeVisible();

    const searchInput = page.getByLabel('Search complexes');
    await searchInput.fill('Mova');
    const responsePromise = page.waitForResponse(/\/api\/v1\/complexes/);
    await page.getByRole('button', { name: 'Search' }).click();
    await responsePromise;

    const viewLink = page.getByRole('link', { name: /View/ });
    await expect(viewLink.or(page.getByText('No active complexes match your search.'))).toBeVisible();
    const complexCount = await viewLink.count();

    if (complexCount === 0) {
      await expect(page.getByText('No active complexes match your search.')).toBeVisible();
      return;
    }

    await viewLink.first().click();
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
  });

  test('complex detail can filter courts by sport and view availability', async ({ page }) => {
    await page.goto('/complexes');
    await page.waitForResponse(/\/api\/v1\/complexes/);

    const viewLink = page.getByRole('link', { name: /View/ });
    await expect(viewLink.first()).toBeVisible();
    const complexCount = await viewLink.count();

    test.skip(complexCount === 0, 'No active complexes are available in the test environment.');

    await viewLink.first().click();

    const courtSelect = page.getByLabel('Court');
    const dateInput = page.getByLabel('Date');

    await expect(courtSelect).toBeVisible({ timeout: 5000 });
    await expect(dateInput).toBeVisible({ timeout: 5000 });

    const courtOptionsCount = await page.locator('[role="option"]').count();

    if (courtOptionsCount > 1) {
      await courtSelect.click();
      await page.getByRole('option').nth(1).click();

      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      const isoDate = tomorrow.toISOString().split('T')[0];

      await dateInput.fill(isoDate);

      const hasSlots = await page.getByText('to').first().isVisible().catch(() => false);
      const noSlotsMessage = page.getByText(
        'No available slots for the selected date.'
      );

      if (hasSlots) {
        await expect(page.getByText('Check availability')).toBeVisible();
      } else {
        await expect(noSlotsMessage.or(page.getByText('Select a court and date to view available slots.'))).toBeVisible();
      }
    }
  });

  test.use({ timezoneId: 'Europe/Berlin' });
  test('availability date is driven by the complex time zone, not the visitor browser time zone', async ({ page, request }) => {
    const complex = await getFirstActiveComplex(request);
    test.skip(!complex, 'No active complexes are available in the test environment.');

    await page.goto(`/complexes/${complex.id}`);

    const dateInput = page.getByLabel('Date');
    await expect(dateInput).toBeVisible({ timeout: 5000 });

    const expected = await expectedTodayInTimeZone(page, complex.timeZoneId);
    await expect(dateInput).toHaveValue(expected);
  });

  test.describe('DST boundary', () => {
    test.use({ timezoneId: 'America/New_York', locale: 'en-US' });

    test('a DST-observing time zone can be queried on the spring-forward transition', async ({ page, request }) => {
      const complex = await getFirstActiveComplex(request);
      test.skip(!complex, 'No active complexes are available in the test environment.');

      await page.goto(`/complexes/${complex.id}`);

      const courtSelect = page.getByLabel('Court');
      const dateInput = page.getByLabel('Date');

      await expect(courtSelect).toBeVisible({ timeout: 5000 });

      await courtSelect.click();

      const courtOptionsCount = await page.getByRole('option').count();
      test.skip(courtOptionsCount === 0, 'No courts are available in the test environment.');

      await page.getByRole('option').nth(0).click();

      await dateInput.fill('2026-03-08');

      const noSlotsMessage = page.getByText('No available slots for the selected date.');
      const checkAvailability = page.getByText('Check availability');

      await expect(noSlotsMessage.or(checkAvailability)).toBeVisible({ timeout: 10000 });
    });
  });
});
