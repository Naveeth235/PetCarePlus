// e2e/utils/auth.ts
// Purpose: Helper functions for authentication in E2E tests
// Features: Login helpers for different user roles

import { Page, expect } from '@playwright/test';

export interface TestCredentials {
  email: string;
  password: string;
}

// Test credentials - using actual registered users
export const testCredentials = {
  owner: {
    email: 'user12345@petcare.com',
    password: 'user12345'
  },
  admin: {
    email: 'admin@petcare.com', 
    password: 'AdminPass123'
  },
  vet: {
    email: 'vet@test.com',
    password: 'password123'
  }
};

/**
 * Logs in a user with the provided credentials
 */
export async function login(page: Page, credentials: TestCredentials) {
  await page.goto('/login');
  
  // Wait for the login form to be visible
  await page.waitForSelector('input[placeholder*="Email"], input[name="email"], input[type="email"]');
  
  // Fill in credentials - try different selectors to be flexible
  const emailInput = page.locator('input[placeholder*="Email"], input[name="email"], input[type="email"]').first();
  const passwordInput = page.locator('input[placeholder*="Password"], input[name="password"], input[type="password"]').first();
  
  await emailInput.fill(credentials.email);
  await passwordInput.fill(credentials.password);
  
  // Submit form - try different button selectors
  const submitButton = page.locator('button[type="submit"], button:has-text("Log In"), button:has-text("Login"), button:has-text("Sign In")').first();
  await submitButton.click();
  
  // Wait for login to complete - check for dashboard or redirect
  await page.waitForURL(url => !url.pathname.includes('/login'), { timeout: 10000 });
}

/**
 * Logs in as an owner user
 */
export async function loginAsOwner(page: Page) {
  await login(page, testCredentials.owner);
  // Verify we're on the owner dashboard
  await expect(page).toHaveURL(/\/owner/);
}

/**
 * Logs in as an admin user  
 */
export async function loginAsAdmin(page: Page) {
  await login(page, testCredentials.admin);
  // Verify we're on the admin dashboard
  await expect(page).toHaveURL(/\/admin/);
}

/**
 * Logs out the current user
 */
export async function logout(page: Page) {
  // Look for logout button/link - try different selectors
  const logoutSelector = 'button:has-text("Logout"), button:has-text("Log Out"), a:has-text("Logout"), [data-testid="logout"]';
  
  try {
    await page.locator(logoutSelector).first().click();
    await page.waitForURL(url => url.pathname.includes('/login') || url.pathname === '/', { timeout: 5000 });
  } catch (error) {
    // If logout button not found, navigate directly to login
    console.warn('Logout button not found, navigating to login page');
    await page.goto('/login');
  }
}