using ModbusLab.Api.Endpoints;
using ModbusLab.Application.Devices;
using ModbusLab.Application.Modbus;
using ModbusLab.Infrastructure;
using ModbusLab.Infrastructure.Persistence;
using ModbusLab.Api.BackgroundServices;
using ModbusLab.Api.Realtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHostedService<RandomRegisterSimulationWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<DeviceQueryService>();
builder.Services.AddScoped<ModbusRegisterService>();

var app = builder.Build();

await DatabaseSeeder.SeedAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapDeviceEndpoints();
app.MapModbusEndpoints();

app.MapHub<ModbusHub>("/hubs/modbus");

app.Run();