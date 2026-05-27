# ModbusLab

[![CI](https://github.com/kotyasmol/ModbusLab/actions/workflows/ci.yml/badge.svg)](https://github.com/kotyasmol/ModbusLab/actions/workflows/ci.yml)

ModbusLab is a fullstack industrial monitoring and test automation demo app. It simulates a Modbus-like device lab with protected operations, role-based access control, PostgreSQL persistence, realtime register updates, audit logs, and an operator dashboard.

The project is built as a portfolio-ready product slice: backend API, frontend UI, database migrations, Docker Compose, CI checks, unit tests, and Playwright E2E coverage.

## Screenshots

<table>
  <tr>
    <td width="50%">
      <strong>Dashboard</strong><br />
      <img src="docs/screenshots/dashboard.jpg" alt="Dashboard" width="100%" />
    </td>
    <td width="50%">
      <strong>Monitoring</strong><br />
      <img src="docs/screenshots/monitoring.jpg" alt="Monitoring" width="100%" />
    </td>
  </tr>
  <tr>
    <td width="50%">
      <strong>Testing</strong><br />
      <img src="docs/screenshots/testing.jpg" alt="Testing" width="100%" />
    </td>
    <td width="50%">
      <strong>Audit Logs</strong><br />
      <img src="docs/screenshots/audit.jpg" alt="Audit Logs" width="100%" />
    </td>
  </tr>
</table>

## Why This Project Exists

Industrial devices are often checked manually: an operator reads registers, writes control values, compares measurements with allowed ranges, and records the result. ModbusLab turns that workflow into a web platform:

- monitor simulated slave devices and registers;
- read and write register values with permission checks;
- run repeatable test scenarios;
- inspect passed and failed test steps;
- export test run reports;
- audit user actions;
- manage users and roles as an admin.

## Core Features

### Auth And RBAC

- JWT authentication.
- Local users with hashed passwords.
- Roles: `Viewer`, `Engineer`, `Admin`.
- Admin user management:
  - list users;
  - create users;
  - change roles;
  - enable or disable accounts.
- Safety rules:
  - disabled users cannot login;
  - public registration is controlled by configuration;
  - login/register rate limiting;
  - the last active Admin cannot be disabled or demoted;
  - an Admin cannot disable their own account.
- Swagger Bearer auth support.

### Device Management

- Engineer/Admin device administration page.
- Create device types.
- Create slave devices with unique Modbus slave addresses.
- Enable or disable devices.
- Create register definitions for a device type.
- Automatically initialize register values for existing devices of that type.

### Device Monitoring

- Multiple seeded demo devices.
- Register definitions with access modes and allowed ranges.
- Read/write register operations.
- Modbus operation log.
- SignalR realtime register updates.

### Test Automation

- Test profiles with ordered steps.
- Supported step types:
  - write register;
  - delay;
  - check register range.
- Passing and intentionally failing demo scenarios.
- Test run history.
- CSV report export.

### Audit And Diagnostics

- Audit log for auth, user management, register writes, and testing actions.
- Audit filtering by action, user, result, and date range in the API.
- Health endpoints for API and database.
- Docker healthchecks for PostgreSQL, API, and frontend.

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
| `admin` | `Admin123!` | `Admin` | Full access, users, audit logs, test profile management |
| `engineer` | `Engineer123!` | `Engineer` | Monitoring, register writes, test execution |
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
| `POST` | `/api/test-profiles/{profileId}/run` | Run test profile |
| `GET` | `/api/test-runs` | Latest test runs |
| `GET` | `/api/test-runs/{runId}/report.csv` | Export CSV report |
| `GET` | `/api/audit-logs` | Filtered audit logs |
| `GET` | `/api/health` | API health |
| `GET` | `/api/health/db` | Database health |

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

## Portfolio Highlights

- Fullstack feature work across API, UI, database, Docker, and CI.
- Realistic RBAC with admin user management.
- Engineer/Admin device management with validation and audit logging.
- EF Core migrations and PostgreSQL persistence.
- Realtime updates with SignalR.
- Audit logging and health diagnostics.
- Unit tests and browser-level E2E tests.
- Docker Compose setup that starts the full product locally.

## Roadmap

- Refresh tokens and active session management.
- Edit/delete workflows for device types, slave devices, and registers.
- OpenTelemetry traces and structured request logging.
- Background queue for long-running test execution.
- Live test progress over SignalR.
- PDF test reports.
- Integration tests against PostgreSQL.
