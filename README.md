# PetCarePlus

## Stack

- **Backend**: ASP.NET Core (.NET 9), EF Core (MySQL), ASP.NET Identity + JWT
- **Frontend**: React + TypeScript + Vite + Tailwind
- **DB**: MySQL 8

## Architecture (Backend)

Modular monolith with 4 projects:

- **PetCare.Api** → API controllers, middleware, DI bootstrap
- **PetCare.Application** → use-cases (commands/queries), DTOs, validation
- **PetCare.Domain** → entities, enums, core rules (no EF)
- **PetCare.Infrastructure** → EF Core (DbContext + Migrations), Identity, JWT

## Features Implemented (Sprint 1)

- **Authentication & Roles**

  - Pet Owner self-registration (`/api/auth/register-owner`)
  - Login for all roles → returns JWT with role claim
  - Profile update for logged-in users (`/api/users/me`)
  - Admin-only endpoint to create Vet accounts (`/api/admin/users/vets`)

- **Security**

  - JWT Bearer tokens (no cookies, no redirects)
  - Role-based authorization: Owner, Vet, Admin
  - Password hashing via ASP.NET Identity

- **Frontend**

  - Connected login & signup pages to API
  - Owner registration and login flow working end-to-end

## Prereqs

- .NET SDK 9
- Node.js 20+
- MySQL 8

## Run (dev)

```bash
# API
cd backend/src/PetCare.Api
ASPNETCORE_URLS=http://localhost:5002 dotnet run
# Swagger: http://localhost:5002/swagger

# Web
cd frontend
cp .env.example .env   # adjust backend API URL if needed
npm install
npm run dev
# http://localhost:5173
```

## Default Accounts (Dev)

- **Admin**

  - Email: `admin@petcare.com`
  - Password: `AdminPass123`

- Create Owners via `/auth/register-owner`
- Create Vets via Admin endpoint `/admin/users/vets`
