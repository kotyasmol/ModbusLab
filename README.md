

# ModbusLab

ModbusLab is a fullstack pet project for monitoring and testing Modbus-like industrial devices. The project demonstrates a small production-style stack: protected API endpoints, PostgreSQL persistence, a React dashboard, realtime updates, and basic automated test profiles for device registers.

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

## Features

- Device monitoring dashboard
- Register read/write operations
- Modbus operation journal
- Test profiles with configurable steps
- Test run execution and history
- Realtime register updates via SignalR
- JWT authentication with local users

## Run Locally

Start PostgreSQL:

```powershell
docker compose up -d
```

Restore and run the API:

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

Default URLs:

- API/Swagger: `http://localhost:5199/swagger`
- Frontend: `http://localhost:5173`

## Demo Account

For local development only:

- Login: `admin`
- Password: `Admin123!`
- Role: `Admin`

Do not use this account or the development JWT secret in production.

## Quality Checks

```powershell
dotnet restore
dotnet build
npm install --prefix frontend
npm run build --prefix frontend
npm run lint --prefix frontend
```

There are currently no dedicated test projects in the repository.

## Roadmap

- User roles and permissions
- Audit log for register writes and test runs
- Advanced visual test designer
- Exportable reports
- CI/CD pipeline
