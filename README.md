# ModbusLab

[![CI](https://github.com/kotyasmol/ModbusLab/actions/workflows/ci.yml/badge.svg)](https://github.com/kotyasmol/ModbusLab/actions/workflows/ci.yml)

ModbusLab is a fullstack industrial monitoring and test automation demo app. It simulates a Modbus-like device laboratory with protected operations, role-based access control, PostgreSQL persistence, realtime register updates, audit logs, device administration, user administration, and an operator dashboard.

The project is built as a portfolio-ready product slice: backend API, frontend UI, database migrations, Docker Compose, CI checks, backend tests, and Playwright E2E coverage.

## Screenshots

### Login

<img src="docs/screenshots/login.png" alt="Login page" width="100%" />

### Dashboard

<img src="docs/screenshots/dashboard.png" alt="Dashboard page" width="100%" />

### Device monitoring

<img src="docs/screenshots/monitoring.png" alt="Device monitoring page" width="100%" />

### Test automation

<img src="docs/screenshots/testing.png" alt="Test automation page" width="100%" />

### Device administration

<img src="docs/screenshots/deviceadmin.png" alt="Device administration page" width="100%" />

### User administration

<img src="docs/screenshots/users.png" alt="User administration page" width="100%" />

### Audit logs

<img src="docs/screenshots/auditlogs.png" alt="Audit logs page" width="100%" />

## Why This Project Exists

Industrial devices are often checked manually: an operator reads registers, writes control values, compares measurements with allowed ranges, starts separate test operations, and records the result. This approach is slow, hard to scale, and easy to break through human error.

ModbusLab turns that workflow into a web platform:

- monitor simulated slave devices and their registers;
- read and write register values with permission checks;
- run repeatable test scenarios;
- track live test execution progress;
- inspect passed and failed test steps;
- export test run reports;
- audit user actions;
- manage users, roles, devices, device types, and register definitions.

## What This Project Demonstrates

This repository is not just a frontend mockup. It includes a complete vertical slice of a production-style application:

- React + TypeScript frontend with protected pages and role-based UI;
- ASP.NET Core backend with Minimal API endpoints;
- PostgreSQL database with Entity Framework Core migrations;
- JWT authentication and authorization policies;
- SignalR realtime updates;
- background workers for simulated register changes and queued test execution;
- Docker Compose setup for local startup;
- automated CI checks;
- backend unit tests and browser-level E2E tests.

## Core Features

### Authentication And RBAC

- JWT authentication.
- Local users with hashed passwords.
- Roles: `Viewer`, `Engineer`, `Admin`.
- Protected frontend routes and role-aware navigation.
- Swagger Bearer authentication support.
- Public registration controlled by configuration.
- Rate limiting for login and registration.
- Safety rules:
  - disabled users cannot log in;
  - the last active Admin cannot be disabled or demoted;
  - an Admin cannot disable their own account.

### User Administration

Admin users can:

- view the user list;
- create new users;
- change user roles;
- enable or disable user accounts;
- review user-related actions through audit logs.

### Device Administration

Engineer/Admin users can:

- create device types;
- create slave devices with unique Modbus slave addresses;
- enable or disable devices;
- create register definitions for a device type;
- initialize register values for existing devices of that type.

### Device Monitoring

- Multiple seeded demo devices.
- Register definitions with access modes and allowed ranges.
- Read and write register operations.
- Permission checks for write operations.
- Modbus operation log.
- SignalR realtime register updates.

### Test Automation

- Test profiles with ordered steps.
- Supported step types:
  - write register;
  - delay;
  - check register range.
- Passing and intentionally failing demo scenarios.
- Queued background test execution.
- Live test progress over SignalR.
- Test run history.
- CSV report export.

### Audit And Diagnostics

- Audit log for authentication, user management, register writes, device management, and testing actions.
- Audit filtering by action, user, result, and date range in the API.
- Health endpoints for API and database.
- Docker healthchecks for PostgreSQL, API, and frontend.

## Application Pages

| Page | Purpose | Typical Access |
| --- | --- | --- |
| Login | Sign in or switch to registration | Public |
| Dashboard | High-level overview of devices, latest logs, profiles, and test runs | Viewer / Engineer / Admin |
| Monitoring | Register values, read/write operations, and Modbus log | Viewer can read, Engineer/Admin can write |
| Testing | Test profiles, test execution, progress, history, and CSV reports | Engineer / Admin |
| Device Admin | Device types, devices, register definitions, and device status | Engineer / Admin |
| Users | User creation, role changes, account status management | Admin |
| Audit Logs | Security and business action history | Admin |

## Tech Stack

### Backend

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core
- PostgreSQL
- SignalR
- JWT Bearer Authentication
- xUnit

### Frontend

- React
- TypeScript
- Vite
- TanStack Query
- SignalR Client
- Playwright
- Custom CSS

### Infrastructure

- Docker Compose
- PostgreSQL 17
- GitHub Actions
- Swagger / OpenAPI

## Architecture

```text
React + TypeScript UI
        |
        | HTTP + JWT / SignalR
        v
ASP.NET Core Minimal API
        |
        | Application services
        v
Domain entities and business rules
        |
        | EF Core repositories
        v
PostgreSQL
```

Repository layout:

```text
src/
  ModbusLab.Api/             HTTP API, auth, endpoints, SignalR, background services
  ModbusLab.Application/     Application services and use cases
  ModbusLab.Domain/          Domain entities and business rules
  ModbusLab.Infrastructure/  EF Core, PostgreSQL, repositories, migrations

tests/
  ModbusLab.Tests/           Backend unit tests

frontend/
  src/                       React application
  e2e/                       Playwright E2E tests

```

## Demo Accounts

Use these accounts for local development only.

| Login | Password | Role | Access |
| --- | --- | --- | --- |
| `admin` | `Admin123!` | `Admin` | Full access, users, audit logs, device management, test profile management |
| `engineer` | `Engineer123!` | `Engineer` | Monitoring, register writes, test execution, device management |
| `viewer` | `Viewer123!` | `Viewer` | Read-only monitoring and history |

## Quick Start With Docker

Build and run PostgreSQL, API, and frontend:

```powershell
docker compose up --build
```

Default URLs:

| Service | URL |
| --- | --- |
| Frontend | `http://localhost:5173` |
| Swagger | `http://localhost:8080/swagger` |
| API health | `http://localhost:8080/api/health` |
| Database health | `http://localhost:8080/api/health/db` |
| PostgreSQL | `localhost:5433` |

Stop containers:

```powershell
docker compose down
```

Reset local database volume:

```powershell
docker compose down -v
```

## Local Development

Start PostgreSQL only:

```powershell
docker compose up -d postgres
```

Run the API:

```powershell
dotnet restore
dotnet run --project src/ModbusLab.Api
```

Run the frontend:

```powershell
npm install --prefix frontend
npm run dev --prefix frontend
```

## Quality Checks

Backend:

```powershell
dotnet restore
dotnet build
dotnet test
```

Frontend:

```powershell
npm install --prefix frontend
npm run build --prefix frontend
npm run lint --prefix frontend
```

E2E:

```powershell
npm run e2e --prefix frontend
```

Docker:

```powershell
docker compose config
docker compose build
```

GitHub Actions runs backend, frontend, Docker, and Playwright E2E checks on push and pull requests to `main`.

## Important API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/auth/login` | Login and receive JWT |
| `POST` | `/api/auth/register` | Public registration when enabled |
| `GET` | `/api/auth/me` | Current authenticated user |
| `GET` | `/api/users` | Admin user list |
| `POST` | `/api/users` | Admin user creation |
| `PATCH` | `/api/users/{userId}/role` | Admin role change |
| `PATCH` | `/api/users/{userId}/status` | Admin enable/disable |
| `GET` | `/api/devices` | Device list |
| `GET` | `/api/devices/{deviceId}/registers` | Device registers |
| `GET` | `/api/device-management/types` | Device type list |
| `POST` | `/api/device-management/types` | Create device type |
| `POST` | `/api/device-management/devices` | Create slave device |
| `PATCH` | `/api/device-management/devices/{deviceId}/status` | Enable or disable device |
| `POST` | `/api/device-management/registers` | Create register definition |
| `POST` | `/api/modbus/read` | Read register |
| `POST` | `/api/modbus/write` | Write register |
| `GET` | `/api/modbus/logs` | Modbus operation log |
| `GET` | `/api/test-profiles` | Test profiles |
| `POST` | `/api/test-profiles` | Create test profile |
| `POST` | `/api/test-profiles/{profileId}/run` | Run test profile |
| `GET` | `/api/test-runs` | Latest test runs |
| `GET` | `/api/test-runs/{runId}/report.csv` | Export CSV report |
| `GET` | `/api/audit-logs` | Filtered audit logs |
| `GET` | `/api/health` | API health |
| `GET` | `/api/health/db` | Database health |
| `GET` | `/hubs/modbus` | SignalR hub for realtime updates |

## Portfolio Highlights

- Fullstack feature work across API, UI, database, Docker, and CI.
- Realistic RBAC with Admin, Engineer, and Viewer roles.
- Protected write operations for industrial-style register control.
- Engineer/Admin device management with validation and audit logging.
- EF Core migrations and PostgreSQL persistence.
- Realtime updates with SignalR.
- Background workers for simulated device activity and test execution.
- Audit logging and health diagnostics.
- Unit tests and browser-level E2E tests.
- Docker Compose setup that starts the full product locally.

## Roadmap

- Refresh tokens and active session management.
- Edit/delete workflows for device types, slave devices, and registers.
- OpenTelemetry traces and structured request logging.
- More detailed dashboard analytics.
- PDF test reports.
- Integration tests against PostgreSQL.
- Real Modbus RTU/TCP adapter behind the current application service contracts.
