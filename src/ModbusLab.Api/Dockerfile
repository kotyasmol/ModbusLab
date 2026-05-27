FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ModbusLab.Domain/ModbusLab.Domain.csproj src/ModbusLab.Domain/
COPY src/ModbusLab.Application/ModbusLab.Application.csproj src/ModbusLab.Application/
COPY src/ModbusLab.Infrastructure/ModbusLab.Infrastructure.csproj src/ModbusLab.Infrastructure/
COPY src/ModbusLab.Api/ModbusLab.Api.csproj src/ModbusLab.Api/

RUN dotnet restore src/ModbusLab.Api/ModbusLab.Api.csproj

COPY src/ src/

RUN dotnet publish src/ModbusLab.Api/ModbusLab.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=3s --retries=6 \
    CMD curl -fsS http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "ModbusLab.Api.dll"]
