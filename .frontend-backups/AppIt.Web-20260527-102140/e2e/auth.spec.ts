import { expect, test } from '@playwright/test';

test.describe('Auth flow', () => {
  test('auth page renders login form', async ({ page }) => {
    await page.goto('/auth');
    await expect(page.locator('input[name="loginEmail"]')).toBeVisible();
    await expect(page.locator('input[name="loginPassword"]')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Sign In' })).toBeVisible();
  });
});
