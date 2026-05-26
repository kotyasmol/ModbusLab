# ModbusLab

ModbusLab is a fullstack pet project for monitoring and testing Modbus-like industrial devices. It shows a compact production-style stack: protected API endpoints, role-based access control, PostgreSQL persistence, audit logs, realtime updates, and a React operator dashboard.

## Stack

- .NET 10
- ASP.NET Core Minimal API
- EF Core
- PostgreSQL
- React
- TypeScript
- TanStack Query
- SignalR
- Docker Compose
- GitHub Actions

## Features

- JWT authentication with local users
- RBAC roles: Viewer, Engineer, Admin
- User audit log for auth, register writes, and test actions
- Device monitoring dashboard
- Register read/write operations
- Modbus operation journal
- Test profiles with configurable steps
- Test run execution and history
- Realtime register updates via SignalR
- Swagger with JWT Authorize support
- Full Docker Compose setup for PostgreSQL, API, and frontend
- CI workflow for backend and frontend builds

## Demo Accounts

For local development only:

| Login | Password | Role |
| --- | --- | --- |
| `admin` | `Admin123!` | `Admin` |
| `engineer` | `Engineer123!` | `Engineer` |
| `viewer` | `Viewer123!` | `Viewer` |

Do not use these accounts or the development JWT secret in production.

## Local Development

Start PostgreSQL only:

```powershell
docker compose up -d postgres
```

Restore, migrate, and run the API:

```powershell
dotnet restore
dotnet ef database update --project src/ModbusLab.Infrastructure --startup-project src/ModbusLab.Api
dotnet run --project src/ModbusLab.Api
```

Start the frontend:

```powershell
cd frontend
npm install
npm run dev
```

Default local URLs:

- API/Swagger: `http://localhost:5199/swagger`
- Frontend: `http://localhost:5173`

## Full Docker Run

Build and run PostgreSQL, API, and frontend:

```powershell
docker compose --profile full up --build
```

Docker URLs:

- API/Swagger: `http://localhost:8080/swagger`
- Frontend: `http://localhost:5173`

## Quality Checks

```powershell
dotnet restore
dotnet build
dotnet test
npm install --prefix frontend
npm run build --prefix frontend
npm run lint --prefix frontend
docker compose config
```

GitHub Actions runs the same backend and frontend build checks on push and pull requests to `main`.

## Roadmap

- Exportable reports
- Visual test designer
- Observability and health checks
- Advanced device management
