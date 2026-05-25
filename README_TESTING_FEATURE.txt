Что добавляется этим набором файлов
===================================

Это первый крупный функциональный модуль для ModbusLab: тестовые профили и запуск автоматизированных тестов.

После установки появится:
- backend-модуль тестовых профилей;
- таблицы test_profiles, test_steps, test_runs, test_step_results;
- API:
  - GET  /api/test-profiles
  - GET  /api/test-profiles/{id}
  - POST /api/test-profiles
  - POST /api/test-profiles/{id}/steps
  - POST /api/test-profiles/{id}/run
  - GET  /api/test-runs
  - GET  /api/test-runs/{id}
- frontend-вкладка "Тестовые сценарии";
- создание профиля;
- добавление шагов:
  - запись регистра;
  - пауза;
  - проверка регистра на диапазон;
- запуск профиля;
- отображение результата и истории запусков.

Как применить
=============

1. Распакуй архив в корень репозитория ModbusLab с заменой файлов.

Корень у тебя примерно:
C:\Users\kotyo\Desktop\FT\ModbusLab

2. Важно: некоторые файлы заменяются:
- src/ModbusLab.Api/Program.cs
- src/ModbusLab.Api/appsettings.Development.json
- src/ModbusLab.Infrastructure/Persistence/ModbusLabDbContext.cs
- src/ModbusLab.Infrastructure/Persistence/DatabaseSeeder.cs
- src/ModbusLab.Infrastructure/DependencyInjection.cs
- frontend/src/App.tsx
- frontend/src/App.css
- frontend/src/shared/api/apiClient.ts
- frontend/src/shared/api/modbusHubConnection.ts

3. После замены создай миграцию:

dotnet tool install --global dotnet-ef

dotnet ef migrations add AddTestingModule --project .\src\ModbusLab.Infrastructure\ModbusLab.Infrastructure.csproj --startup-project .\src\ModbusLab.Api\ModbusLab.Api.csproj

4. Запусти базу:

docker start modbuslab-postgres

или:

docker compose up -d

5. Запусти backend:

dotnet run --project .\src\ModbusLab.Api\ModbusLab.Api.csproj

6. Запусти frontend:

cd .\frontend
npm install
npm run dev

7. Открой:
Frontend: http://localhost:5173
Swagger:  http://localhost:5199/swagger/index.html
