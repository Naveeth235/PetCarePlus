// e2e/utils/test-data.ts
// Purpose: Test data generators and constants for E2E tests
// Features: Unique test data generation to avoid conflicts

/**
 * Generates unique test data to avoid conflicts between test runs
 */
export function generateTestData() {
  const timestamp = Date.now();
  const randomId = Math.floor(Math.random() * 1000);
  
  return {
    appointmentReason: `Annual Checkup - E2E Test ${timestamp}`,
    appointmentNotes: `Automated test appointment created at ${new Date().toISOString()}`,
    uniqueId: `${timestamp}-${randomId}`,
    preferredDate: getNextWeekDate(),
  };
}

/**
 * Gets a date one week from now in YYYY-MM-DD format
 */
function getNextWeekDate(): string {
  const date = new Date();
  date.setDate(date.getDate() + 7);
  return date.toISOString().split('T')[0];
}

/**
 * Common test selectors that might change
 */
export const selectors = {
  // Login page
  emailInput: 'input[placeholder*="Email"], input[name="email"], input[type="email"]',
  passwordInput: 'input[placeholder*="Password"], input[name="password"], input[type="password"]',
  loginButton: 'button[type="submit"], button:has-text("Log In"), button:has-text("Login"), button:has-text("Sign In")',
  
  // Navigation
  appointmentsLink: 'a:has-text("Appointment"), nav a[href*="appointment"], [data-testid="appointments-nav"]',
  requestAppointmentLink: 'a:has-text("Request Appointment"), button:has-text("Request Appointment"), a:has-text("Request"), button:has-text("Request"), [data-testid="request-appointment"]',
  manageAppointmentsLink: 'a:has-text("Manage"), nav a[href*="admin/appointment"], [data-testid="manage-appointments"]',
  
  // Forms
  petSelect: 'select[name*="pet"], select:has(option:has-text("pet")), [data-testid="pet-select"]',
  reasonSelect: 'select[name*="reason"], [data-testid="reason-select"]',
  dateInput: 'input[type="date"], input[name*="date"], [data-testid="preferred-date"]',
  notesTextarea: 'textarea[name*="note"], textarea[placeholder*="note"], [data-testid="notes"]',
  submitButton: 'button[type="submit"], button:has-text("Submit"), [data-testid="submit-appointment"]',
  
  // Admin actions
  appointmentCard: '.appointment-card, [data-testid="appointment-card"], .appointment-item',
  approveButton: 'button:has-text("Approve"), [data-testid="approve-button"]',
  
  // Notifications
  notificationBell: '[data-testid="notification-bell"], .notification-bell, button:has([data-testid*="bell"])',
  notificationBadge: '.badge, .notification-badge, [data-testid="notification-badge"]',
  notificationDropdown: '.notification-dropdown, [data-testid="notification-dropdown"]',
};

export default {
  generateTestData,
  selectors,
};