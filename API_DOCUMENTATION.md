# PetCarePlus API Documentation

## Table of Contents

1. [Overview](#overview)
2. [Authentication](#authentication)
3. [Authorization & Roles](#authorization--roles)
4. [Base URL & Environments](#base-url--environments)
5. [API Endpoints](#api-endpoints)
   - [Authentication](#authentication-endpoints)
   - [Users](#users-endpoints)
   - [Pets](#pets-endpoints)
   - [Appointments](#appointments-endpoints)
   - [Notifications](#notifications-endpoints)
   - [Medical Records](#medical-records-endpoints)
   - [Vaccinations](#vaccinations-endpoints)
   - [Treatments](#treatments-endpoints)
   - [Prescriptions](#prescriptions-endpoints)
   - [Inventory](#inventory-endpoints)
   - [Admin - User Management](#admin-user-management-endpoints)
6. [Data Models](#data-models)
7. [Error Handling](#error-handling)
8. [Rate Limiting](#rate-limiting)
9. [Testing](#testing)

---

## Overview

PetCarePlus is a comprehensive pet care management system built with ASP.NET Core (.NET 9) and React. The API follows RESTful principles and uses JWT for authentication.

**Architecture:**
- Modular monolith with clean architecture
- 4 projects: API, Application, Domain, Infrastructure
- EF Core with MySQL database
- ASP.NET Identity for user management
- JWT Bearer authentication

**Tech Stack:**
- Backend: ASP.NET Core 9, EF Core, MySQL 8
- Frontend: React + TypeScript + Vite + Tailwind CSS
- Authentication: ASP.NET Identity + JWT

---

## Authentication

All protected endpoints require a valid JWT token in the `Authorization` header.

### Request Header Format

```http
Authorization: Bearer <your_jwt_token>
```

### Token Acquisition

Tokens are obtained through the `/api/auth/login` endpoint. Tokens include:
- User ID
- Email
- Role(s) (OWNER, VET, ADMIN)
- Expiration time

### Token Storage

Frontend stores the JWT in `localStorage` with key `APP_AT`.

---

## Authorization & Roles

The system implements role-based access control with three primary roles:

| Role | Description | Capabilities |
|------|-------------|--------------|
| **OWNER** | Pet owners | - Register pets<br>- Request appointments<br>- View own pet records<br>- Manage profile |
| **VET** | Veterinarians | - View assigned appointments<br>- Create medical records<br>- Prescribe medications<br>- View all pets |
| **ADMIN** | System administrators | - All Owner & Vet capabilities<br>- Manage users (create vets, update users)<br>- Approve/reject appointments<br>- System-wide access |

---

## Base URL & Environments

### Development
```
API Base URL: http://localhost:5002
Frontend: http://localhost:5173
Swagger UI: http://localhost:5002/swagger
```

### Production
```
API Base URL: https://api.petcareplus.com
Frontend: https://petcareplus.com
```

---

## API Endpoints

### Authentication Endpoints

#### Register Owner (Public)

Create a new pet owner account.

```http
POST /api/auth/register-owner
```

**Request Body:**
```json
{
  "fullName": "John Smith",
  "email": "john.smith@example.com",
  "password": "SecurePass123!"
}
```

**Response:** `201 Created`
```json
{
  "message": "Registration successful"
}
```

**Error Responses:**
- `400 Bad Request` - Validation failed
- `409 Conflict` - Email already exists

---

#### Login (Public)

Authenticate and receive JWT token.

```http
POST /api/auth/login
```

**Request Body:**
```json
{
  "email": "admin@petcare.com",
  "password": "AdminPass123"
}
```

**Response:** `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-11-19T10:30:00Z",
  "userId": "abc123",
  "fullName": "Admin User",
  "email": "admin@petcare.com",
  "roles": ["ADMIN"]
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid credentials
- `403 Forbidden` - Account inactive

---

#### Get Current User (Protected)

Get authenticated user details and roles.

```http
GET /api/auth/me
GET /api/users/me  (alternative)
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "userId": "abc123",
  "fullName": "John Smith",
  "email": "john.smith@example.com",
  "roles": ["OWNER"]
}
```

**Error Responses:**
- `401 Unauthorized` - Invalid or missing token

---

### Users Endpoints

#### Update Own Profile (Protected)

Update current user's profile information.

```http
PUT /api/users/me
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Request Body:**
```json
{
  "fullName": "John A. Smith",
  "email": "newemail@example.com"
}
```

**Response:** `200 OK`
```json
{
  "userId": "abc123",
  "fullName": "John A. Smith",
  "email": "newemail@example.com",
  "message": "Profile updated successfully"
}
```

**Error Responses:**
- `400 Bad Request` - Validation failed
- `401 Unauthorized` - Not authenticated
- `409 Conflict` - Email already in use

---

### Pets Endpoints

#### Get All Pets (Admin/Vet Only)

Retrieve all pets in the system.

```http
GET /api/pets
```

**Headers:**
```http
Authorization: Bearer <admin_or_vet_token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "pet-guid-1",
    "name": "Max",
    "species": "Dog",
    "breed": "Golden Retriever",
    "dateOfBirth": "2020-03-15",
    "color": "Golden",
    "weight": 30.5,
    "ownerUserId": "owner-guid",
    "ownerName": "John Smith",
    "isActive": true
  }
]
```

**Authorization:**
- Roles: `VET`, `ADMIN`

---

#### Get Pet by ID (Protected)

Get details of a specific pet.

```http
GET /api/pets/{id}
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "id": "pet-guid-1",
  "name": "Max",
  "species": "Dog",
  "breed": "Golden Retriever",
  "dateOfBirth": "2020-03-15",
  "color": "Golden",
  "weight": 30.5,
  "medicalNotes": "Allergic to chicken",
  "ownerUserId": "owner-guid",
  "ownerName": "John Smith",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-11-18T14:22:00Z"
}
```

**Authorization:**
- VETs and Admins can access any pet
- Owners can only access their own pets

**Error Responses:**
- `404 Not Found` - Pet not found
- `403 Forbidden` - Not authorized to view this pet

---

#### Get Pets by Owner (Protected)

Get all pets belonging to a specific owner.

```http
GET /api/pets/owner/{ownerId}
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "pet-guid-1",
    "name": "Max",
    "species": "Dog",
    "breed": "Golden Retriever",
    "ownerUserId": "owner-guid",
    "ownerName": "John Smith",
    "isActive": true
  }
]
```

**Authorization:**
- VETs and Admins can view any owner's pets
- Owners can only view their own pets

---

#### Get My Pets (Protected)

Get current user's pets.

```http
GET /api/pets/my-pets
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "pet-guid-1",
    "name": "Max",
    "species": "Dog",
    "breed": "Golden Retriever",
    "dateOfBirth": "2020-03-15",
    "isActive": true
  }
]
```

---

#### Get Users for Pet Assignment (Admin Only)

Get list of owners for assigning pets.

```http
GET /api/pets/users/selection
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "user-guid-1",
    "fullName": "John Smith",
    "email": "john@example.com",
    "role": "Owner"
  }
]
```

**Authorization:**
- Roles: `ADMIN`

---

#### Create Pet (Admin Only)

Create a new pet record.

```http
POST /api/pets
```

**Headers:**
```http
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Max",
  "species": "Dog",
  "breed": "Golden Retriever",
  "dateOfBirth": "2020-03-15",
  "color": "Golden",
  "weight": 30.5,
  "medicalNotes": "Allergic to chicken",
  "ownerUserId": "owner-guid"
}
```

**Response:** `201 Created`
```json
{
  "id": "pet-guid-1",
  "name": "Max",
  "species": "Dog",
  "breed": "Golden Retriever",
  "dateOfBirth": "2020-03-15",
  "color": "Golden",
  "weight": 30.5,
  "medicalNotes": "Allergic to chicken",
  "ownerUserId": "owner-guid",
  "isActive": true
}
```

**Authorization:**
- Roles: `ADMIN`

---

#### Update Pet (Admin Only)

Update existing pet information.

```http
PUT /api/pets/{id}
```

**Headers:**
```http
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Max",
  "species": "Dog",
  "breed": "Golden Retriever",
  "dateOfBirth": "2020-03-15",
  "color": "Golden",
  "weight": 32.0,
  "medicalNotes": "Allergic to chicken and beef",
  "isActive": true
}
```

**Response:** `200 OK`

**Authorization:**
- Roles: `ADMIN`

---

#### Delete Pet (Admin Only)

Delete a pet record.

```http
DELETE /api/pets/{id}
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response:** `204 No Content`

**Authorization:**
- Roles: `ADMIN`

**Error Responses:**
- `404 Not Found` - Pet not found

---

#### Assign Pet to New Owner (Admin Only)

Transfer pet ownership.

```http
POST /api/pets/{id}/assign
```

**Headers:**
```http
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "petId": "pet-guid-1",
  "newOwnerUserId": "new-owner-guid"
}
```

**Response:** `200 OK`

**Authorization:**
- Roles: `ADMIN`

---

### Appointments Endpoints

#### Create Appointment Request (Owner/Admin)

Owner creates a new appointment request.

```http
POST /api/appointments
```

**Headers:**
```http
Authorization: Bearer <owner_or_admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "petId": "pet-guid-1",
  "requestedDateTime": "2025-11-25T10:00:00Z",
  "reasonForVisit": "Annual Checkup",
  "notes": "Max has been limping slightly"
}
```

**Response:** `201 Created`
```json
{
  "id": "appointment-guid-1",
  "petId": "pet-guid-1",
  "petName": "Max",
  "ownerUserId": "owner-guid",
  "ownerName": "John Smith",
  "requestedDateTime": "2025-11-25T10:00:00Z",
  "reasonForVisit": "Annual Checkup",
  "notes": "Max has been limping slightly",
  "status": "Pending",
  "statusDisplayName": "Pending Approval",
  "canBeCancelled": true,
  "requiresAction": false,
  "createdAt": "2025-11-18T15:30:00Z"
}
```

**Validation:**
- Appointment must be for a future date/time
- Pet must belong to the requesting owner

**Authorization:**
- Roles: `OWNER`, `ADMIN`

---

#### Get Appointment by ID (Protected)

Get details of a specific appointment.

```http
GET /api/appointments/{id}
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "id": "appointment-guid-1",
  "petId": "pet-guid-1",
  "petName": "Max",
  "ownerUserId": "owner-guid",
  "ownerName": "John Smith",
  "vetUserId": "vet-guid",
  "vetName": "Dr. Jane Smith",
  "requestedDateTime": "2025-11-25T10:00:00Z",
  "actualDateTime": "2025-11-25T10:00:00Z",
  "reasonForVisit": "Annual Checkup",
  "notes": "Max has been limping slightly",
  "adminNotes": "Approved - assigned to Dr. Smith",
  "status": "Approved",
  "statusDisplayName": "Approved",
  "canBeCancelled": false,
  "requiresAction": false,
  "createdAt": "2025-11-18T15:30:00Z",
  "updatedAt": "2025-11-18T16:00:00Z"
}
```

**Authorization:**
- Owners can view their own appointments
- Admins and Vets can view any appointment

---

#### Get My Appointments (Owner/Admin)

Get all appointments for the current owner.

```http
GET /api/appointments/my
```

**Headers:**
```http
Authorization: Bearer <owner_or_admin_token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "appointment-guid-1",
    "petName": "Max",
    "ownerName": "John Smith",
    "vetName": "Dr. Jane Smith",
    "requestedDateTime": "2025-11-25T10:00:00Z",
    "reasonForVisit": "Annual Checkup",
    "status": "Approved",
    "statusDisplayName": "Approved"
  }
]
```

**Authorization:**
- Roles: `OWNER`, `ADMIN`

---

#### Get All Appointments (Admin Only)

Get all appointments in the system.

```http
GET /api/appointments
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "appointment-guid-1",
    "petName": "Max",
    "ownerName": "John Smith",
    "vetName": "Dr. Jane Smith",
    "requestedDateTime": "2025-11-25T10:00:00Z",
    "status": "Approved"
  }
]
```

**Authorization:**
- Roles: `ADMIN`

---

#### Get Pending Appointments (Admin Only)

Get appointments awaiting approval.

```http
GET /api/appointments/pending
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "appointment-guid-2",
    "petName": "Bella",
    "ownerName": "Mary Johnson",
    "requestedDateTime": "2025-11-26T14:00:00Z",
    "reasonForVisit": "Vaccination",
    "status": "Pending",
    "requiresAction": true
  }
]
```

**Authorization:**
- Roles: `ADMIN`

---

#### Update Appointment Status (Admin Only)

Approve or reject appointment requests with vet assignment.

```http
PUT /api/appointments/{id}/status
```

**Headers:**
```http
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request Body (Approve):**
```json
{
  "status": "Approved",
  "adminNotes": "Approved - regular checkup scheduled",
  "vetUserId": "vet-guid"
}
```

**Request Body (Reject):**
```json
{
  "status": "Cancelled",
  "adminNotes": "Unable to accommodate at requested time"
}
```

**Response:** `200 OK`
```json
{
  "id": "appointment-guid-1",
  "status": "Approved",
  "statusDisplayName": "Approved",
  "vetName": "Dr. Jane Smith",
  "adminNotes": "Approved - regular checkup scheduled"
}
```

**Business Logic:**
- Only pending appointments can be updated
- Approving creates notifications for owner and assigned vet
- Cancelling creates notification for owner
- Vet assignment required when approving

**Authorization:**
- Roles: `ADMIN`

---

#### Get Assigned Appointments (Vet/Admin)

Get appointments assigned to the current vet.

```http
GET /api/appointments/assigned
```

**Headers:**
```http
Authorization: Bearer <vet_or_admin_token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "appointment-guid-1",
    "petName": "Max",
    "ownerName": "John Smith",
    "requestedDateTime": "2025-11-25T10:00:00Z",
    "reasonForVisit": "Annual Checkup",
    "status": "Approved"
  }
]
```

**Authorization:**
- Roles: `VET`, `ADMIN`

---

#### Get Approved Appointments (Vet/Admin)

Get all approved appointments (for dashboard).

```http
GET /api/appointments/approved
```

**Headers:**
```http
Authorization: Bearer <vet_or_admin_token>
```

**Response:** `200 OK`

**Authorization:**
- Roles: `VET`, `ADMIN`

---

#### Get Appointment Summary Report (Admin Only)

Get comprehensive appointment statistics and workload metrics.

```http
GET /api/appointments/summary-report
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response:** `200 OK`
```json
{
  "generatedAt": "2025-11-18T10:30:00Z",
  "reportPeriod": "As of November 18, 2025",
  "upcomingAppointmentsCount": 15,
  "pendingAppointmentsCount": 5,
  "pastAppointmentsCount": 120,
  "totalAppointmentsCount": 140,
  "completedAppointmentsCount": 100,
  "cancelledAppointmentsCount": 15,
  "noShowAppointmentsCount": 5,
  "averageAppointmentsPerDay": 4.2,
  "busiestDayOfWeek": "Wednesday",
  "peakAppointmentHour": 14,
  "upcomingAppointments": [],
  "pendingAppointments": [],
  "pastAppointments": []
}
```

**Authorization:**
- Roles: `ADMIN`

---

### Notifications Endpoints

#### Get My Notifications (Protected)

Get current user's notifications.

```http
GET /api/notifications
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "notification-guid-1",
    "userId": "user-guid",
    "type": "AppointmentApproved",
    "typeDisplayName": "Appointment Approved",
    "title": "Appointment Approved",
    "message": "Your appointment for Max on 2025-11-25 has been approved",
    "data": "{\"appointmentId\":\"appointment-guid-1\",\"petName\":\"Max\"}",
    "isRead": false,
    "isRecent": true,
    "createdAt": "2025-11-18T16:00:00Z"
  }
]
```

---

#### Get Unread Count (Protected)

Get count of unread notifications for badge display.

```http
GET /api/notifications/unread-count
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "unreadCount": 3
}
```

---

#### Get Notification by ID (Protected)

Get specific notification details.

```http
GET /api/notifications/{id}
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`

**Authorization:**
- Users can only access their own notifications

---

#### Mark Notification as Read (Protected)

Mark a single notification as read.

```http
PUT /api/notifications/{id}/read
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "id": "notification-guid-1",
  "isRead": true,
  "readAt": "2025-11-18T16:30:00Z"
}
```

**Authorization:**
- Users can only mark their own notifications

---

#### Mark All Notifications as Read (Protected)

Mark all user's notifications as read.

```http
PUT /api/notifications/mark-all-read
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "message": "All notifications marked as read"
}
```

---

### Medical Records Endpoints

#### Create Medical Record (Vet/Admin)

Create a new medical record for a pet.

```http
POST /api/medicalrecords
```

**Headers:**
```http
Authorization: Bearer <vet_or_admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "petId": "pet-guid-1",
  "recordType": "Checkup",
  "recordDate": "2025-11-18T10:00:00Z",
  "title": "Annual Wellness Exam",
  "description": "Routine checkup - all vitals normal",
  "notes": "Weight: 30.5kg, Temperature: 38.5°C"
}
```

**Response:** `201 Created`
```json
{
  "id": "record-guid-1",
  "petId": "pet-guid-1",
  "recordType": "Checkup",
  "recordDate": "2025-11-18T10:00:00Z",
  "title": "Annual Wellness Exam",
  "description": "Routine checkup - all vitals normal",
  "notes": "Weight: 30.5kg, Temperature: 38.5°C",
  "createdByUserId": "vet-guid",
  "createdAt": "2025-11-18T10:15:00Z"
}
```

**Record Types:**
- Checkup
- Emergency
- Surgery
- LabResults
- Imaging
- Other

**Authorization:**
- Roles: `VET`, `ADMIN`

---

#### Get Medical Records by Pet (Protected)

Get all medical records for a specific pet.

```http
GET /api/medicalrecords/pet/{petId}
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "record-guid-1",
    "petId": "pet-guid-1",
    "recordType": "Checkup",
    "recordDate": "2025-11-18T10:00:00Z",
    "title": "Annual Wellness Exam",
    "description": "Routine checkup - all vitals normal"
  }
]
```

**Authorization:**
- VETs and Admins can view all records
- Owners can only view their own pet's records

---

### Vaccinations Endpoints

#### Create Vaccination Record (Vet/Admin)

Create a new vaccination record.

```http
POST /api/vaccinations
```

**Headers:**
```http
Authorization: Bearer <vet_or_admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "petId": "pet-guid-1",
  "vaccineName": "Rabies",
  "vaccinationDate": "2025-11-18T10:00:00Z",
  "nextDueDate": "2026-11-18T10:00:00Z",
  "batchNumber": "VAC-2025-1234",
  "manufacturer": "Zoetis",
  "notes": "No adverse reactions"
}
```

**Response:** `201 Created`

**Authorization:**
- Roles: `VET`, `ADMIN`

---

#### Get Vaccination Report (Protected)

Get vaccination history for a pet.

```http
GET /api/vaccinations/pet/{petId}/report
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "petId": "pet-guid-1",
  "petName": "Max",
  "vaccinations": [
    {
      "vaccineName": "Rabies",
      "vaccinationDate": "2025-11-18T10:00:00Z",
      "nextDueDate": "2026-11-18T10:00:00Z",
      "status": "Current"
    }
  ],
  "overdueVaccinations": [],
  "upcomingVaccinations": []
}
```

**Authorization:**
- VETs and Admins can view all records
- Owners can view their own pet's records

---

### Treatments Endpoints

#### Create Treatment Record (Vet/Admin)

Create a new treatment record.

```http
POST /api/treatments
```

**Headers:**
```http
Authorization: Bearer <vet_or_admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "petId": "pet-guid-1",
  "treatmentType": "Medical",
  "diagnosis": "Mild ear infection",
  "treatmentDescription": "Antibiotic ear drops administered",
  "treatmentDate": "2025-11-18T10:00:00Z",
  "followUpDate": "2025-11-25T10:00:00Z",
  "medications": "Otomax ear drops",
  "instructions": "Apply 5 drops to affected ear twice daily",
  "notes": "Recheck in 7 days"
}
```

**Response:** `201 Created`

**Authorization:**
- Roles: `VET`, `ADMIN`

---

#### Get Treatment History (Protected)

Get treatment history for a pet.

```http
GET /api/treatments/pet/{petId}/history
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
{
  "petId": "pet-guid-1",
  "petName": "Max",
  "treatments": [
    {
      "treatmentType": "Medical",
      "diagnosis": "Mild ear infection",
      "treatmentDate": "2025-11-18T10:00:00Z",
      "followUpDate": "2025-11-25T10:00:00Z"
    }
  ],
  "upcomingFollowUps": []
}
```

**Authorization:**
- VETs and Admins can view all records
- Owners can view their own pet's records

---

### Prescriptions Endpoints

#### Create Prescription (Vet/Admin)

Create a new prescription.

```http
POST /api/prescriptions
```

**Headers:**
```http
Authorization: Bearer <vet_or_admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "petId": "pet-guid-1",
  "medicationName": "Amoxicillin",
  "dosage": "250mg",
  "frequency": "Twice daily",
  "prescribedDate": "2025-11-18T10:00:00Z",
  "startDate": "2025-11-18T10:00:00Z",
  "endDate": "2025-11-28T10:00:00Z",
  "durationDays": 10,
  "instructions": "Give with food",
  "notes": "Complete full course"
}
```

**Response:** `201 Created`

**Authorization:**
- Roles: `VET`, `ADMIN`

---

### Inventory Endpoints

#### Get All Inventory Items (Protected)

Get all inventory items.

```http
GET /api/inventory
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`
```json
[
  {
    "id": "inventory-guid-1",
    "name": "Dog Food - Premium",
    "category": "Food",
    "quantity": 50,
    "unit": "bags",
    "price": 45.99,
    "supplier": "Premium Pet Foods Inc.",
    "description": "High-quality dog food",
    "photoUrl": "http://localhost:5002/images/inventory/dog-food.jpg"
  }
]
```

---

#### Get Inventory Item by ID (Protected)

Get specific inventory item details.

```http
GET /api/inventory/{id}
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Response:** `200 OK`

---

#### Search Inventory (Protected)

Search inventory items by name, category, supplier, or description.

```http
GET /api/inventory/search?q=dog+food
```

**Headers:**
```http
Authorization: Bearer <token>
```

**Query Parameters:**
- `q` (required) - Search query string

**Response:** `200 OK`

---

#### Create Inventory Item (Admin/Vet)

Create a new inventory item.

```http
POST /api/inventory
```

**Headers:**
```http
Authorization: Bearer <admin_or_vet_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Dog Food - Premium",
  "category": "Food",
  "quantity": 50,
  "unit": "bags",
  "price": 45.99,
  "supplier": "Premium Pet Foods Inc.",
  "description": "High-quality dog food",
  "photoUrl": "http://localhost:5002/images/inventory/dog-food.jpg"
}
```

**Response:** `201 Created`

**Error Responses:**
- `400 Bad Request` - Duplicate item (if item with same name exists)

---

#### Update Inventory Item (Admin/Vet)

Update existing inventory item.

```http
PUT /api/inventory/{id}
```

**Headers:**
```http
Authorization: Bearer <admin_or_vet_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Dog Food - Premium",
  "category": "Food",
  "quantity": 45,
  "unit": "bags",
  "price": 47.99,
  "supplier": "Premium Pet Foods Inc.",
  "description": "High-quality dog food"
}
```

**Response:** `204 No Content`

---

#### Delete Inventory Item (Admin/Vet)

Delete an inventory item.

```http
DELETE /api/inventory/{id}
```

**Headers:**
```http
Authorization: Bearer <admin_or_vet_token>
```

**Response:** `204 No Content`

---

#### Upload Inventory Photo (Admin/Vet)

Upload a photo for inventory items.

```http
POST /api/inventory/upload-photo
```

**Headers:**
```http
Authorization: Bearer <admin_or_vet_token>
Content-Type: multipart/form-data
```

**Request Body (Form Data):**
- `file` - Image file (JPEG, PNG, etc.)

**Response:** `200 OK`
```json
{
  "url": "http://localhost:5002/images/inventory/abc123.jpg"
}
```

---

### Admin User Management Endpoints

#### Create Vet (Admin Only)

Create a new veterinarian account.

```http
POST /api/admin/users/vets
```

**Headers:**
```http
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "fullName": "Dr. Jane Smith",
  "email": "jane.smith@petcare.com",
  "password": "SecureVetPass123!"
}
```

**Response:** `201 Created`
```json
{
  "userId": "vet-guid-1",
  "fullName": "Dr. Jane Smith",
  "email": "jane.smith@petcare.com",
  "roles": ["VET"]
}
```

**Authorization:**
- Roles: `ADMIN`

**Error Responses:**
- `400 Bad Request` - Validation failed
- `409 Conflict` - Email already exists

---

#### List Vets (Admin Only)

Get paginated list of veterinarians with search.

```http
GET /api/admin/users/vets?search={query}&page={page}&pageSize={size}
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Query Parameters:**
- `search` (optional) - Search by name or email
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10, max: 100) - Items per page

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "vet-guid-1",
      "fullName": "Dr. Jane Smith",
      "email": "jane.smith@petcare.com"
    }
  ],
  "total": 25,
  "page": 1,
  "pageSize": 10
}
```

**Authorization:**
- Roles: `ADMIN`

---

#### Get Vet by ID (Admin Only)

Get specific vet details.

```http
GET /api/admin/users/vets/{id}
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Response:** `200 OK`
```json
{
  "id": "vet-guid-1",
  "fullName": "Dr. Jane Smith",
  "email": "jane.smith@petcare.com"
}
```

**Authorization:**
- Roles: `ADMIN`

---

#### List All Users (Admin Only)

Get paginated list of all users with optional role filtering.

```http
GET /api/admin/users?role={role}&search={query}&page={page}&pageSize={size}
```

**Headers:**
```http
Authorization: Bearer <admin_token>
```

**Query Parameters:**
- `role` (optional) - Filter by role: Admin, Vet, Owner
- `search` (optional) - Search by name or email
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 10, max: 100) - Items per page

**Response:** `200 OK`
```json
{
  "items": [
    {
      "id": "user-guid-1",
      "fullName": "John Smith",
      "email": "john@example.com",
      "accountStatus": "Active",
      "isActive": true,
      "roles": ["OWNER"]
    }
  ],
  "total": 150,
  "page": 1,
  "pageSize": 10
}
```

**Authorization:**
- Roles: `ADMIN`

---

#### Update User (Admin Only)

Update user profile and account status.

```http
PUT /api/admin/users/{id}
```

**Headers:**
```http
Authorization: Bearer <admin_token>
Content-Type: application/json
```

**Request Body:**
```json
{
  "fullName": "John A. Smith",
  "email": "newemail@example.com",
  "accountStatus": "Active"
}
```

**Response:** `200 OK`
```json
{
  "id": "user-guid-1",
  "fullName": "John A. Smith",
  "email": "newemail@example.com",
  "accountStatus": "Active",
  "isActive": true,
  "roles": ["OWNER"]
}
```

**Validation:**
- FullName must be 2-100 characters
- Email must be unique
- AccountStatus: `Active` or `Inactive`
- Admins cannot deactivate themselves

**Authorization:**
- Roles: `ADMIN`

---

## Data Models

### User

```typescript
{
  id: string;              // GUID
  fullName: string;
  email: string;
  accountStatus: "Active" | "Inactive";
  roles: string[];         // ["OWNER"] | ["VET"] | ["ADMIN"]
  createdAt: DateTime;
}
```

### Pet

```typescript
{
  id: string;              // GUID
  name: string;
  species: string;         // Dog, Cat, etc.
  breed: string;
  dateOfBirth: DateTime;
  color: string;
  weight: number;          // in kg
  medicalNotes?: string;
  ownerUserId: string;
  ownerName: string;
  isActive: boolean;
  createdAt: DateTime;
  updatedAt: DateTime;
}
```

### Appointment

```typescript
{
  id: string;                    // GUID
  petId: string;
  petName: string;
  ownerUserId: string;
  ownerName: string;
  vetUserId?: string;
  vetName?: string;
  requestedDateTime: DateTime;
  actualDateTime?: DateTime;
  reasonForVisit: string;
  notes?: string;
  adminNotes?: string;
  status: "Pending" | "Approved" | "Cancelled" | "Completed" | "NoShow";
  statusDisplayName: string;
  canBeCancelled: boolean;
  requiresAction: boolean;
  createdAt: DateTime;
  updatedAt: DateTime;
}
```

### Notification

```typescript
{
  id: string;                    // GUID
  userId: string;
  type: "AppointmentApproved" | "AppointmentCancelled" | "AppointmentAssigned" | 
        "AppointmentReminder" | "SystemMessage";
  typeDisplayName: string;
  title: string;
  message: string;
  data?: string;                 // JSON string with additional data
  isRead: boolean;
  isRecent: boolean;             // Created within last 24 hours
  createdAt: DateTime;
  readAt?: DateTime;
}
```

### MedicalRecord

```typescript
{
  id: string;                    // GUID
  petId: string;
  recordType: "Checkup" | "Emergency" | "Surgery" | "LabResults" | "Imaging" | "Other";
  recordDate: DateTime;
  title: string;
  description?: string;
  notes?: string;
  createdByUserId: string;
  createdAt: DateTime;
  updatedAt: DateTime;
}
```

### Vaccination

```typescript
{
  id: string;                    // GUID
  petId: string;
  vaccineName: string;
  vaccinationDate: DateTime;
  nextDueDate?: DateTime;
  batchNumber?: string;
  manufacturer?: string;
  notes?: string;
  createdByUserId: string;
  createdAt: DateTime;
}
```

### Treatment

```typescript
{
  id: string;                    // GUID
  petId: string;
  treatmentType: string;
  diagnosis: string;
  treatmentDescription?: string;
  treatmentDate: DateTime;
  followUpDate?: DateTime;
  medications?: string;
  instructions?: string;
  notes?: string;
  createdByUserId: string;
  createdAt: DateTime;
}
```

### Prescription

```typescript
{
  id: string;                    // GUID
  petId: string;
  medicationName: string;
  dosage: string;
  frequency: string;
  prescribedDate: DateTime;
  startDate?: DateTime;
  endDate?: DateTime;
  durationDays?: number;
  instructions?: string;
  notes?: string;
  prescribedByUserId: string;
  createdAt: DateTime;
}
```

### InventoryItem

```typescript
{
  id: string;                    // GUID
  name: string;
  category: string;
  quantity: number;
  unit: string;
  price: number;
  supplier?: string;
  description?: string;
  photoUrl?: string;
  createdAt: DateTime;
  updatedAt: DateTime;
}
```

---

## Error Handling

### Standard Error Response Format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Error Title",
  "status": 400,
  "detail": "Detailed error message"
}
```

### HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful GET, PUT requests |
| 201 | Created | Successful POST that creates a resource |
| 204 | No Content | Successful DELETE or PUT with no response body |
| 400 | Bad Request | Invalid request data, validation errors |
| 401 | Unauthorized | Missing or invalid authentication token |
| 403 | Forbidden | Authenticated but not authorized for this resource |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource conflict (e.g., duplicate email) |
| 500 | Internal Server Error | Server-side error |

### Common Error Scenarios

#### 401 Unauthorized
```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication token is missing or invalid"
}
```

#### 403 Forbidden
```json
{
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to access this resource"
}
```

#### 400 Validation Error
```json
{
  "title": "Validation failed",
  "status": 400,
  "detail": "Full name must be between 2 and 100 characters.",
  "errors": {
    "fullName": ["Must be between 2 and 100 characters"]
  }
}
```

#### 409 Conflict
```json
{
  "title": "Email already in use",
  "status": 409,
  "detail": "An account with this email already exists."
}
```

---

## Rate Limiting

Currently not implemented. Future versions may include:
- Per-user rate limits
- Per-endpoint rate limits
- Rate limit headers in responses

---

## Testing

### Default Test Accounts

#### Admin Account
```
Email: admin@petcare.com
Password: AdminPass123
Roles: ADMIN
```

Use this account to:
- Create vets
- Manage all users
- Approve appointments
- Access all system features

### Testing Tools

1. **Swagger UI**
   - URL: `http://localhost:5002/swagger`
   - Interactive API documentation
   - Test endpoints directly in browser

2. **Postman Collection**
   - Import collection from `/docs/PetCarePlus.postman_collection.json`
   - Pre-configured requests for all endpoints
   - Environment variables for easy testing

3. **cURL Examples**

**Get Admin Token:**
```bash
curl -X POST http://localhost:5002/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@petcare.com","password":"AdminPass123"}'
```

**Create Appointment:**
```bash
curl -X POST http://localhost:5002/api/appointments \
  -H "Authorization: Bearer <YOUR_TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "petId": "pet-guid",
    "requestedDateTime": "2025-11-25T10:00:00Z",
    "reasonForVisit": "Annual Checkup",
    "notes": "Regular checkup"
  }'
```

**Get My Notifications:**
```bash
curl -X GET http://localhost:5002/api/notifications \
  -H "Authorization: Bearer <YOUR_TOKEN>"
```

### Integration Tests

Run backend integration tests:
```bash
cd backend
dotnet test
```

### Frontend Testing

Run Playwright E2E tests:
```bash
cd frontend
npm run test:e2e
```

---

## Versioning

Current API Version: **v1**

Version is not currently included in URL path. Future versions may use:
- URL versioning: `/api/v2/appointments`
- Header versioning: `Accept: application/vnd.petcareplus.v2+json`

---

## Support & Contact

For API support, issues, or feature requests:
- GitHub Issues: [Link to repository issues]
- Email: support@petcareplus.com
- Documentation: [Link to docs]

---

## Changelog

### Version 1.0 (Current)
- Initial API release
- Authentication & Authorization
- User Management (Owners, Vets, Admins)
- Pet Management
- Appointment System with approval workflow
- Notification System
- Medical Records, Vaccinations, Treatments, Prescriptions
- Inventory Management
- Admin User Management

---

## Future Enhancements

Planned features for upcoming releases:
- [ ] WebSocket support for real-time notifications
- [ ] Rate limiting implementation
- [ ] API versioning
- [ ] Appointment reminders (automated)
- [ ] Payment integration
- [ ] Report generation (PDF exports)
- [ ] Bulk operations
- [ ] Advanced search and filtering
- [ ] Calendar integration
- [ ] Mobile app support
- [ ] Two-factor authentication

---

**Last Updated:** November 18, 2025  
**API Version:** 1.0  
**Documentation Version:** 1.0
