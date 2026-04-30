using ModbusLab.Application.Abstractions;
using ModbusLab.Application.Devices;
using ModbusLab.Application.Modbus;
using ModbusLab.Infrastructure;
using ModbusLab.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<DeviceQueryService>();
builder.Services.AddScoped<ModbusRegisterService>();

var app = builder.Build();

await DatabaseSeeder.SeedAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/devices", async (
    DeviceQueryService deviceQueryService,
    CancellationToken cancellationToken) =>
{
    var devices = await deviceQueryService.GetDevicesAsync(cancellationToken);

    return Results.Ok(devices);
})
.WithTags("Devices")
.WithSummary("Get all Modbus slave devices");

app.MapGet("/api/devices/{deviceId:guid}/registers", async (
    Guid deviceId,
    DeviceQueryService deviceQueryService,
    CancellationToken cancellationToken) =>
{
    var registers = await deviceQueryService.GetDeviceRegistersAsync(
        deviceId,
        cancellationToken);

    return Results.Ok(registers);
})
.WithTags("Devices")
.WithSummary("Get registers for selected device");

app.MapPost("/api/modbus/read", async (
    ReadRegisterRequest request,
    ModbusRegisterService modbusRegisterService,
    CancellationToken cancellationToken) =>
{
    var result = await modbusRegisterService.ReadAsync(
        request,
        cancellationToken);

    return result.IsSuccess
        ? Results.Ok(result)
        : Results.BadRequest(result);
})
.WithTags("Modbus")
.WithSummary("Read Modbus register");

app.MapPost("/api/modbus/write", async (
    WriteRegisterRequest request,
    ModbusRegisterService modbusRegisterService,
    CancellationToken cancellationToken) =>
{
    var result = await modbusRegisterService.WriteAsync(
        request,
        cancellationToken);

    return result.IsSuccess
        ? Results.Ok(result)
        : Results.BadRequest(result);
})
.WithTags("Modbus")
.WithSummary("Write Modbus register");

app.MapGet("/api/modbus/logs", async (
    IModbusLogRepository logRepository,
    CancellationToken cancellationToken) =>
{
    var logs = await logRepository.GetLatestAsync(
        count: 100,
        cancellationToken);

    return Results.Ok(logs);
})
.WithTags("Modbus")
.WithSummary("Get latest Modbus operation logs");

app.Run();