# Test Data Setup Guide

## 🔐 Required Test Users

For the E2E tests to work, you need these users in your database:

### Owner User
- **Email:** `owner@test.com`
- **Password:** `password123`
- **Role:** `OWNER`
- **Requirements:** Must have at least 1 pet registered

### Admin User  
- **Email:** `admin@test.com`
- **Password:** `password123`
- **Role:** `ADMIN`

## 📝 Creating Test Users

### Option 1: Use Your Registration Page
1. Go to `/register` 
2. Create the users with the credentials above
3. For the admin user, you'll need to manually update the role in your database

### Option 2: Database Script (SQL Server)
```sql
-- Insert test owner
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, CreatedAt)
VALUES ('owner@test.com', 'your-hashed-password', 'Test', 'Owner', 'OWNER', GETDATE());

-- Insert test admin  
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, CreatedAt)
VALUES ('admin@test.com', 'your-hashed-password', 'Test', 'Admin', 'ADMIN', GETDATE());

-- Add a test pet for the owner
INSERT INTO Pets (Name, Species, Breed, Age, OwnerId, CreatedAt)
VALUES ('Fido', 'Dog', 'Golden Retriever', 3, 
        (SELECT Id FROM Users WHERE Email = 'owner@test.com'), 
        GETDATE());
```

### Option 3: Update Test Credentials
If you prefer to use existing users, update the credentials in:
`frontend/e2e/utils/auth.ts`

```typescript
export const testCredentials = {
  owner: {
    email: 'your-existing-owner@email.com',
    password: 'your-password'
  },
  admin: {
    email: 'your-existing-admin@email.com', 
    password: 'your-password'
  }
};
```

## 🧪 Verifying Test Setup

Run this quick verification:
```bash
# Navigate to frontend directory
cd frontend

# Run just the smoke test to verify pages load
npm run test:e2e -- --grep "Smoke test"
```

If this passes, your basic setup is working!

## 🚀 Ready to Test!

Once your test users are set up, run the full appointment flow:
```bash
.\run-e2e-tests.ps1 -TestSuite appointment -ShowReport
```

This will open the beautiful HTML report perfect for your presentation! 🎯