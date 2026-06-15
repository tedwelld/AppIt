import { expect, test } from '@playwright/test';

test.describe('Auth flow', () => {
  test('auth page renders login form', async ({ page }) => {
    await page.goto('/auth');
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="password"]')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Sign In' })).toBeVisible();
  });

  test('seeded admin can log in through the frontend', async ({ page }) => {
    await page.goto('/auth/login');
    await page.locator('input[name="email"]').fill('admin@appit.com');
    await page.locator('input[name="password"]').fill('Admin@2026');
    await page.getByRole('button', { name: 'Sign In' }).click();

    await expect(page).toHaveURL(/\/admin\/dashboard$/);
  });
});
