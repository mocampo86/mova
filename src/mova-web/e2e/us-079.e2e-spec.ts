import { test, expect } from '@playwright/test';

const API_BASE_URL = process.env.E2E_API_BASE_URL ?? 'http://localhost:5098';

test.describe('US-079 Search and select a user when creating a manual reservation', () => {
  test('administrator searches for a user by email, name, and phone number and creates a manual reservation', async ({ page }) => {
    await page.route(new RegExp(`${API_BASE_URL}/api/v1/complexes/complex-1/courts/[^/]+/price-quote(\\?.*)?$`), (route) => {
      if (route.request().method() !== 'GET') {
        route.continue();
        return;
      }

      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalAmount: 1200,
          currency: 'UYU',
          isFullyCovered: true,
          lines: [
            {
              startAt: '2026-01-01T00:00:00Z',
              endAt: '2026-01-01T01:00:00Z',
              hourlyRate: 1200,
              amount: 1200
            }
          ],
          gaps: []
        })
      });
    });

    await page.goto('/admin/complex/complex-1/reservations');

    await page.getByRole('button', { name: 'Create reservation' }).click();
    await expect(page.getByRole('dialog', { name: 'Create manual reservation' })).toBeVisible();

    const userInput = page.getByRole('combobox', { name: 'User' });
    await userInput.fill('john@example.com');
    await expect(page.getByText('John Doe (john@example.com)')).toBeVisible();
    await page.getByText('John Doe (john@example.com)').click();

    await userInput.clear();
    await userInput.fill('Jane Smith');
    await expect(page.getByText('Jane Smith (jane@example.com)')).toBeVisible();
    await page.getByText('Jane Smith (jane@example.com)').click();

    await userInput.clear();
    await userInput.fill('+54 11 1234 5678');
    await expect(page.getByText('John Doe (john@example.com)')).toBeVisible();
    await page.getByText('John Doe (john@example.com)').click();

    await page.getByLabel('Court').click();
    await page.getByRole('option', { name: 'Court One' }).click();

    const now = new Date();
    const start = new Date(now.getTime() + 60 * 60 * 1000);
    start.setMinutes(0, 0, 0);
    const end = new Date(start.getTime() + 60 * 60 * 1000);

    const toLocalDateTime = (date: Date) =>
      `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}T${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;

    await page.getByLabel('Start').fill(toLocalDateTime(start));
    await page.getByLabel('End').fill(toLocalDateTime(end));

    await page.getByRole('button', { name: 'Create' }).click();

    await expect(page.getByRole('dialog', { name: 'Create manual reservation' })).not.toBeVisible();
  });

  test('search dialog can be used to select a user', async ({ page }) => {
    await page.goto('/admin/complex/complex-1/reservations');

    await page.getByRole('button', { name: 'Create reservation' }).click();
    await expect(page.getByRole('dialog', { name: 'Create manual reservation' })).toBeVisible();

    await page.getByRole('button', { name: 'Search' }).click();
    await expect(page.getByRole('dialog', { name: 'Users' })).toBeVisible();

    await page.getByRole('textbox', { name: 'Search by name, email or phone' }).fill('john');
    await page.getByRole('button', { name: 'Search' }).click();

    await expect(page.getByText('John Doe')).toBeVisible();
    await page.getByRole('row', { name: /John Doe/ }).getByRole('button', { name: 'Select' }).click();

    await expect(page.getByRole('combobox', { name: 'User' })).toHaveValue('John Doe (john@example.com)');
  });
});
