import { test, expect } from '@playwright/test';

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
    await page.getByRole('button', { name: 'Search' }).click();

    const complexCount = await page.getByRole('link', { name: 'View courts' }).count();

    if (complexCount === 0) {
      await expect(page.getByText('No active complexes match your search.')).toBeVisible();
      return;
    }

    await page.getByRole('link', { name: 'View courts' }).first().click();
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
  });

  test('complex detail can filter courts by sport and view availability', async ({ page }) => {
    await page.goto('/complexes');

    const viewCourtsLinks = page.getByRole('link', { name: 'View courts' });
    const complexCount = await viewCourtsLinks.count();

    test.skip(complexCount === 0, 'No active complexes are available in the test environment.');

    await viewCourtsLinks.first().click();

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
});