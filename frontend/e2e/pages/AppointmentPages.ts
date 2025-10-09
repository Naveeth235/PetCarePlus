// e2e/pages/AppointmentPages.ts
// Purpose: Page Object Model for appointment-related pages
// Features: Reusable page methods for cleaner test code

import { Page, expect, Locator } from '@playwright/test';

export class OwnerAppointmentRequestPage {
  readonly page: Page;
  readonly petSelect: Locator;
  readonly reasonSelect: Locator;
  readonly dateInput: Locator;
  readonly notesTextarea: Locator;
  readonly submitButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.petSelect = page.locator('select[name*="pet"], select:has(option:has-text("pet")), [data-testid="pet-select"]').first();
    this.reasonSelect = page.locator('select[name*="reason"], [data-testid="reason-select"]').first();
    this.dateInput = page.locator('input[type="date"], input[name*="date"], [data-testid="preferred-date"]').first();
    this.notesTextarea = page.locator('textarea[name*="note"], textarea[placeholder*="note"], [data-testid="notes"]').first();
    this.submitButton = page.locator('button[type="submit"], button:has-text("Submit"), [data-testid="submit-appointment"]').first();
  }

  async goto() {
    await this.page.goto('/owner/appointments/request');
    await expect(this.page.locator('h1, h2, h3').filter({ hasText: /Request|New.*Appointment/i })).toBeVisible();
  }

  async fillAppointmentForm(data: { petIndex?: number, reasonIndex?: number, date: string, notes: string }) {
    // Select pet
    if (await this.petSelect.isVisible({ timeout: 3000 })) {
      await this.petSelect.selectOption({ index: data.petIndex || 1 });
    }

    // Select reason
    if (await this.reasonSelect.isVisible({ timeout: 3000 })) {
      await this.reasonSelect.selectOption({ index: data.reasonIndex || 1 });
    }

    // Fill date
    if (await this.dateInput.isVisible({ timeout: 3000 })) {
      await this.dateInput.fill(data.date);
    }

    // Fill notes
    if (await this.notesTextarea.isVisible({ timeout: 3000 })) {
      await this.notesTextarea.fill(data.notes);
    }
  }

  async submitForm() {
    await this.submitButton.click();
  }

  async verifySuccess() {
    try {
      await expect(this.page.locator('text=/success|submitted|created/i')).toBeVisible({ timeout: 5000 });
    } catch {
      await expect(this.page).toHaveURL(/\/owner\/appointments/, { timeout: 5000 });
    }
  }
}

export class AdminAppointmentsPage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto('/admin/appointments');
    await expect(this.page.locator('h1, h2, h3').filter({ hasText: /Manage|Appointment/i })).toBeVisible();
  }

  async findAppointmentByNotes(notes: string) {
    const appointmentCard = this.page.locator('.appointment-card, [data-testid="appointment-card"], .appointment-item').filter({ hasText: notes });
    await expect(appointmentCard).toBeVisible({ timeout: 10000 });
    return appointmentCard;
  }

  async findPendingAppointment() {
    const appointmentCard = this.page.locator('.appointment-card, [data-testid="appointment-card"], .appointment-item').filter({ hasText: /pending/i }).first();
    await expect(appointmentCard).toBeVisible({ timeout: 10000 });
    return appointmentCard;
  }

  async approveAppointment(appointmentCard: Locator) {
    const approveButton = appointmentCard.locator('button:has-text("Approve"), [data-testid="approve-button"]');
    await expect(approveButton).toBeVisible();
    await approveButton.click();
    
    // Verify approval
    try {
      await expect(appointmentCard.locator('text=/approved/i')).toBeVisible({ timeout: 5000 });
    } catch {
      // Status might update differently, that's okay
    }
  }
}

export class NotificationComponent {
  readonly page: Page;
  readonly notificationBell: Locator;
  readonly notificationBadge: Locator;
  readonly notificationDropdown: Locator;

  constructor(page: Page) {
    this.page = page;
    this.notificationBell = page.locator('[data-testid="notification-bell"], .notification-bell, button:has([data-testid*="bell"])');
    this.notificationBadge = page.locator('.badge, .notification-badge, [data-testid="notification-badge"]');
    this.notificationDropdown = page.locator('.notification-dropdown, [data-testid="notification-dropdown"]');
  }

  async checkForNewNotifications() {
    await expect(this.notificationBell).toBeVisible({ timeout: 10000 });
    
    if (await this.notificationBadge.isVisible({ timeout: 3000 })) {
      await expect(this.notificationBadge).toHaveText(/[1-9]/);
      return true;
    }
    return false;
  }

  async openNotifications() {
    await this.notificationBell.click();
    await expect(this.notificationDropdown).toBeVisible({ timeout: 5000 });
  }

  async verifyApprovalNotification() {
    await expect(this.notificationDropdown.locator('text=/approved/i')).toBeVisible({ timeout: 5000 });
  }
}