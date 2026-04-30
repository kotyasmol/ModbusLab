using ModbusLab.Api.Endpoints;
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

app.MapDeviceEndpoints();
app.MapModbusEndpoints();

app.Run();