# E2E Tests for PetCarePlus Appointment System

## 🚀 Complete Appointment Workflow Testing

This directory contains production-ready end-to-end tests for the appointment booking feature.

### Prerequisites

1. **Backend Running**: Make sure your backend API is running on `http://localhost:5002`
2. **Frontend Running**: Make sure your frontend is running on `http://localhost:5173`
3. **Test Users**: These users are already configured in your database:
   - Owner user: `user12345@petcare.com`
   - Admin user: `admin@petcare.com`
4. **Test Pet**: The owner should have at least one pet in the system (automatically available)

### Running the Test

#### Option 1: Run All E2E Tests
```bash
cd frontend
npm run test:e2e
```

#### Option 2: Run Just the Appointment Flow Test
```bash
cd frontend  
npm run test:e2e:appointment
```

#### Option 3: Run with UI (Interactive Mode)
```bash
cd frontend
npm run test:e2e:ui
```

#### Option 4: Run with Browser Visible (Headed Mode)
```bash
cd frontend
npm run test:e2e:headed
```

### Viewing the Report

After running tests, view the beautiful HTML report:
```bash
cd frontend
npm run test:e2e:report
```

This opens an interactive report perfect for presentations! 🎯

### What the Test Does

The test simulates this complete user journey:

1. **👤 Owner Login**: Logs in as pet owner
2. **📝 Request Appointment**: Fills out and submits appointment form  
3. **👨‍💼 Admin Login**: Logs in as admin (separate session)
4. **✅ Approve Appointment**: Finds and approves the pending appointment
5. **🔔 Notification Check**: Verifies owner receives notification

### Troubleshooting

#### If test fails on login:
- Check if test users exist in your database
- Update credentials in `e2e/utils/auth.ts`

#### If test fails on form filling:
- The test uses flexible selectors to find form elements
- Check if your form field names/IDs match the selectors in `e2e/utils/test-data.ts`

#### If test fails on navigation:
- Make sure your frontend is running on `http://localhost:5173`
- Check if the routing URLs match your actual routes

### Customizing the Test

#### Current Test Credentials
The tests use these real credentials (already configured):
```typescript
export const testCredentials = {
  owner: {
    email: 'user12345@petcare.com',
    password: 'User@12345'
  },
  admin: {
    email: 'admin@petcare.com', 
    password: 'Admin@123'
  }
};
```

#### Update Selectors
Edit `frontend/e2e/utils/test-data.ts` to match your component selectors.

### CI/CD Integration

Add this to your GitHub Actions or CI pipeline:
```yaml
- name: Run E2E Tests
  run: |
    cd frontend
    npm run test:e2e
```

---

## 📊 Perfect for Presentations!

The HTML report includes:
- ✅ Step-by-step test execution
- 📸 Screenshots of each action  
- 🎥 Video recordings of failures
- 📈 Detailed timing information
- 🔍 Interactive trace viewer

This gives you concrete proof that your appointment feature works end-to-end! 🏆