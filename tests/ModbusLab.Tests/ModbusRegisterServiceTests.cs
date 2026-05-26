using ModbusLab.Application.Modbus;
using ModbusLab.Domain.Devices;
using ModbusLab.Domain.Registers;
using ModbusLab.Infrastructure.Persistence;
using ModbusLab.Infrastructure.Persistence.Repositories;
using Xunit;

namespace ModbusLab.Tests;

public sealed class ModbusRegisterServiceTests
{
    [Fact]
    public async Task WriteAsync_ToReadOnlyRegister_ReturnsRejected()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var device = await SeedDeviceAsync(dbContext);
        await SeedRegisterAsync(dbContext, device, 1305, RegisterAccessMode.ReadOnly, 0, 100);
        var service = CreateService(dbContext);

        var result = await service.WriteAsync(new WriteRegisterRequest(1, 1305, 10));

        Assert.False(result.IsSuccess);
        Assert.Equal("Register is read-only.", result.Message);
    }

    [Fact]
    public async Task WriteAsync_WithOutOfRangeValue_ReturnsRejected()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var device = await SeedDeviceAsync(dbContext);
        await SeedRegisterAsync(dbContext, device, 1300, RegisterAccessMode.ReadWrite, 0, 10);
        var service = CreateService(dbContext);

        var result = await service.WriteAsync(new WriteRegisterRequest(1, 1300, 99));

        Assert.False(result.IsSuccess);
        Assert.Equal("Register value is out of allowed range.", result.Message);
    }

    private static ModbusRegisterService CreateService(ModbusLabDbContext dbContext)
    {
        return new ModbusRegisterService(
            new EfDeviceRepository(dbContext),
            new EfRegisterRepository(dbContext),
            new EfModbusLogRepository(dbContext));
    }

    private static async Task<SlaveDevice> SeedDeviceAsync(ModbusLabDbContext dbContext)
    {
        var deviceType = new DeviceType("TestType", "Test device type");
        var device = new SlaveDevice("Test device", 1, deviceType.Id);

        await dbContext.DeviceTypes.AddAsync(deviceType);
        await dbContext.SlaveDevices.AddAsync(device);
        await dbContext.SaveChangesAsync();

        return device;
    }

    private static async Task SeedRegisterAsync(
        ModbusLabDbContext dbContext,
        SlaveDevice device,
        int address,
        RegisterAccessMode accessMode,
        int minValue,
        int maxValue)
    {
        var definition = new RegisterDefinition(
            device.DeviceTypeId,
            address,
            $"Register {address}",
            accessMode,
            minValue: minValue,
            maxValue: maxValue);

        await dbContext.RegisterDefinitions.AddAsync(definition);
        await dbContext.RegisterValues.AddAsync(new RegisterValue(device.Id, definition.Id, minValue));
        await dbContext.SaveChangesAsync();
    }
}
