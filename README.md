# ModbusLab

ModbusLab — fullstack-платформа для симуляции, мониторинга и автоматизированного тестирования Modbus-устройств.

## Что есть в проекте

- Backend: ASP.NET Core, Minimal API, Swagger, SignalR.
- Frontend: React, TypeScript, Vite, TanStack Query.
- Database: PostgreSQL через Docker.
- Infrastructure: EF Core + Npgsql.
- Предметная область: устройства, типы устройств, карты регистров, значения регистров, журнал Modbus-операций.
- Новый модуль: тестовые профили, шаги теста, запуск теста, история запусков и результаты шагов.

## Быстрый запуск

### 1. База данных

```powershell
docker compose up -d
```

### 2. Backend

```powershell
dotnet restore
dotnet run --project .\src\ModbusLab.Api\ModbusLab.Api.csproj
```

Swagger:

```text
http://localhost:5199/swagger/index.html
```

### 3. Frontend

```powershell
cd .\frontend
npm install
npm run dev
```

Frontend:

```text
http://localhost:5173
```

## Если база уже была создана старой версией

Если раньше ты запускала старую версию без тестовых профилей, проще пересоздать volume PostgreSQL:

```powershell
docker compose down -v
docker compose up -d
```

Потом заново запусти backend. Схема и демо-данные создаются автоматически.
