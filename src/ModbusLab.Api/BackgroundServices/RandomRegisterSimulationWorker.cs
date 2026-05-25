using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ModbusLab.Api.Realtime;
using ModbusLab.Domain.Registers;
using ModbusLab.Infrastructure.Persistence;

namespace ModbusLab.Api.BackgroundServices;

public sealed class RandomRegisterSimulationWorker : BackgroundService
{
    private static readonly int[] SimulatedRegisterAddresses = [1305, 1306];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ModbusHub> _hubContext;
    private readonly ILogger<RandomRegisterSimulationWorker> _logger;

    public RandomRegisterSimulationWorker(
        IServiceScopeFactory scopeFactory,
        IHubContext<ModbusHub> hubContext,
        ILogger<RandomRegisterSimulationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateSimulatedRegistersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Application is shutting down.
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "An error occurred while updating simulated Modbus registers.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task UpdateSimulatedRegistersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ModbusLabDbContext>();

        var devices = await dbContext.SlaveDevices
            .Where(device => device.IsEnabled)
            .ToListAsync(cancellationToken);

        if (devices.Count == 0)
            return;

        var registerDefinitions = await dbContext.RegisterDefinitions
            .Where(register =>
                SimulatedRegisterAddresses.Contains(register.Address) &&
                register.AccessMode == RegisterAccessMode.ReadOnly)
            .ToListAsync(cancellationToken);

        if (registerDefinitions.Count == 0)
            return;

        var deviceIds = devices
            .Select(device => device.Id)
            .ToList();

        var registerDefinitionIds = registerDefinitions
            .Select(register => register.Id)
            .ToList();

        var registerValues = await dbContext.RegisterValues
            .Where(value =>
                deviceIds.Contains(value.SlaveDeviceId) &&
                registerDefinitionIds.Contains(value.RegisterDefinitionId))
            .ToListAsync(cancellationToken);

        var events = new List<RegisterValueChangedEvent>();

        foreach (var device in devices)
        {
            var definitionsForDevice = registerDefinitions
                .Where(definition => definition.DeviceTypeId == device.DeviceTypeId);

            foreach (var definition in definitionsForDevice)
            {
                var registerValue = registerValues.FirstOrDefault(value =>
                    value.SlaveDeviceId == device.Id &&
                    value.RegisterDefinitionId == definition.Id);

                var newValue = GenerateValue(definition);

                if (registerValue is null)
                {
                    registerValue = new RegisterValue(
                        device.Id,
                        definition.Id,
                        newValue);

                    await dbContext.RegisterValues.AddAsync(
                        registerValue,
                        cancellationToken);
                }
                else
                {
                    registerValue.UpdateValue(newValue);
                }

                events.Add(new RegisterValueChangedEvent(
                    device.Id,
                    definition.Id,
                    definition.Address,
                    newValue,
                    registerValue.UpdatedAtUtc));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var registerEvent in events)
        {
            await _hubContext.Clients.All.SendAsync(
                "RegisterValueChanged",
                registerEvent,
                cancellationToken);
        }
    }

    private static int GenerateValue(RegisterDefinition definition)
    {
        var value = definition.Address switch
        {
            1305 => Random.Shared.Next(11750, 12251),
            1306 => Random.Shared.Next(180, 421),
            _ => GenerateGenericValue(definition)
        };

        return Clamp(value, definition.MinValue, definition.MaxValue);
    }

    private static int GenerateGenericValue(RegisterDefinition definition)
    {
        if (definition.MinValue.HasValue && definition.MaxValue.HasValue)
            return Random.Shared.Next(definition.MinValue.Value, definition.MaxValue.Value + 1);

        return Random.Shared.Next(0, 1000);
    }

    private static int Clamp(int value, int? minValue, int? maxValue)
    {
        if (minValue.HasValue && value < minValue.Value)
            return minValue.Value;

        if (maxValue.HasValue && value > maxValue.Value)
            return maxValue.Value;

        return value;
    }
}

public sealed record RegisterValueChangedEvent(
    Guid DeviceId,
    Guid RegisterDefinitionId,
    int RegisterAddress,
    int Value,
    DateTime UpdatedAtUtc);
