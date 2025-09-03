# PetCarePlus

## Stack

- Backend: ASP.NET Core (.NET 8/9), EF Core (MySQL)
- Frontend: React + TypeScript + Vite + Tailwind

## Architecture (backend)

Modular monolith with 4 projects:

- **PetCare.Api**: controllers, middleware, DI bootstrap
- **PetCare.Application**: use-cases (commands/queries), DTOs, validation, ports
- **PetCare.Domain**: entities/value objects (no EF)
- **PetCare.Infrastructure**: EF Core (DbContext + Migrations), repositories, Identity/JWT (later)

## Prereqs

- .NET SDK 8/9, Node 20+, MySQL 8

## Run (dev)

```bash
# API
cd backend/src/PetCare.Api
ASPNETCORE_URLS=http://localhost:5002 dotnet run
# Swagger: http://localhost:5002/swagger

# Web
cd frontend
cp .env.example .env   # adjust if needed
npm i && npm run dev
```
