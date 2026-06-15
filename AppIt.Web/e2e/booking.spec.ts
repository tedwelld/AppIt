import { expect, test } from '@playwright/test';

test('admin can open booking capture workspace', async ({ page }) => {
  await page.goto('/auth/login');
  await page.locator('input[name="email"]').fill('admin@appit.com');
  await page.locator('input[name="password"]').fill('Admin@2026');
  await page.getByRole('button', { name: 'Sign In' }).click();

  await page.waitForURL('**/admin/dashboard', { timeout: 30_000 });
  await page.goto('/admin/reservations/booking');
  await expect(page.getByRole('heading', { name: 'Bookings' })).toBeVisible();
});
