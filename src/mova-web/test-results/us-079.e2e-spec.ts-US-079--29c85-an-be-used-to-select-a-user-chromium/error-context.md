# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: us-079.e2e-spec.ts >> US-079 Search and select a user when creating a manual reservation >> search dialog can be used to select a user
- Location: e2e\us-079.e2e-spec.ts:44:3

# Error details

```
Test timeout of 30000ms exceeded.
```

```
Error: locator.click: Test timeout of 30000ms exceeded.
Call log:
  - waiting for getByRole('button', { name: 'Create reservation' })

```

# Page snapshot

```yaml
- generic [ref=e3]:
  - banner [ref=e4]:
    - generic [ref=e5]:
      - link "MOVA | HOME" [ref=e6] [cursor=pointer]:
        - /url: /
      - generic [ref=e7]:
        - link "User login" [ref=e8] [cursor=pointer]:
          - /url: /login?intent=user
        - link "Admin login" [ref=e9] [cursor=pointer]:
          - /url: /login?intent=complex
        - generic [ref=e10]:
          - generic [ref=e11]: Language
          - generic [ref=e12]:
            - combobox "Language" [ref=e13] [cursor=pointer]:
              - paragraph [ref=e17]: English
            - textbox: en
            - group:
              - generic: Language
  - main [ref=e18]:
    - generic [ref=e21]:
      - heading "Welcome to Mova" [level=4] [ref=e22]
      - paragraph [ref=e23]: Sign in with your Google account to continue.
      - generic [ref=e26]:
        - button "Acceder con Google. Se abre en una pestaña nueva" [ref=e28] [cursor=pointer]:
          - generic [ref=e30]: Acceder con Google
        - iframe
      - paragraph [ref=e40]:
        - text: Own a complex?
        - link "Sign in as an owner" [ref=e41] [cursor=pointer]:
          - /url: /login?intent=complex
```

# Test source

```ts
  1  | import { test, expect } from '@playwright/test';
  2  | 
  3  | test.describe('US-079 Search and select a user when creating a manual reservation', () => {
  4  |   test('administrator searches for a user by email, name, and phone number and creates a manual reservation', async ({ page }) => {
  5  |     await page.goto('/admin/complex/complex-1/reservations');
  6  | 
  7  |     await page.getByRole('button', { name: 'Create reservation' }).click();
  8  |     await expect(page.getByRole('dialog', { name: 'Create manual reservation' })).toBeVisible();
  9  | 
  10 |     const userInput = page.getByRole('combobox', { name: 'User' });
  11 |     await userInput.fill('john@example.com');
  12 |     await expect(page.getByText('John Doe (john@example.com)')).toBeVisible();
  13 |     await page.getByText('John Doe (john@example.com)').click();
  14 | 
  15 |     await userInput.clear();
  16 |     await userInput.fill('Jane Smith');
  17 |     await expect(page.getByText('Jane Smith (jane@example.com)')).toBeVisible();
  18 |     await page.getByText('Jane Smith (jane@example.com)').click();
  19 | 
  20 |     await userInput.clear();
  21 |     await userInput.fill('+54 11 1234 5678');
  22 |     await expect(page.getByText('John Doe (john@example.com)')).toBeVisible();
  23 |     await page.getByText('John Doe (john@example.com)').click();
  24 | 
  25 |     await page.getByLabel('Court').click();
  26 |     await page.getByRole('option', { name: 'Court One' }).click();
  27 | 
  28 |     const now = new Date();
  29 |     const start = new Date(now.getTime() + 60 * 60 * 1000);
  30 |     start.setMinutes(0, 0, 0);
  31 |     const end = new Date(start.getTime() + 60 * 60 * 1000);
  32 | 
  33 |     const toLocalDateTime = (date: Date) =>
  34 |       `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}T${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  35 | 
  36 |     await page.getByLabel('Start').fill(toLocalDateTime(start));
  37 |     await page.getByLabel('End').fill(toLocalDateTime(end));
  38 | 
  39 |     await page.getByRole('button', { name: 'Create' }).click();
  40 | 
  41 |     await expect(page.getByRole('dialog', { name: 'Create manual reservation' })).not.toBeVisible();
  42 |   });
  43 | 
  44 |   test('search dialog can be used to select a user', async ({ page }) => {
  45 |     await page.goto('/admin/complex/complex-1/reservations');
  46 | 
> 47 |     await page.getByRole('button', { name: 'Create reservation' }).click();
     |                                                                    ^ Error: locator.click: Test timeout of 30000ms exceeded.
  48 |     await expect(page.getByRole('dialog', { name: 'Create manual reservation' })).toBeVisible();
  49 | 
  50 |     await page.getByRole('button', { name: 'Search' }).click();
  51 |     await expect(page.getByRole('dialog', { name: 'Users' })).toBeVisible();
  52 | 
  53 |     await page.getByRole('textbox', { name: 'Search by name, email or phone' }).fill('john');
  54 |     await page.getByRole('button', { name: 'Search' }).click();
  55 | 
  56 |     await expect(page.getByText('John Doe')).toBeVisible();
  57 |     await page.getByRole('row', { name: /John Doe/ }).getByRole('button', { name: 'Select' }).click();
  58 | 
  59 |     await expect(page.getByRole('combobox', { name: 'User' })).toHaveValue('John Doe (john@example.com)');
  60 |   });
  61 | });
  62 | 
```