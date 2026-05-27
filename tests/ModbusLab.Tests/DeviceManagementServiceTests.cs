using Microsoft.EntityFrameworkCore;
using ModbusLab.Api.Devices;
using ModbusLab.Domain.Devices;
using Xunit;

namespace ModbusLab.Tests;

public sealed class DeviceManagementServiceTests
{
    [Fact]
    public async Task CreateDeviceAsync_CreatesDevice()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var deviceType = await AddDeviceTypeAsync(dbContext);
        var service = new DeviceManagementService(dbContext);

        var device = await service.CreateDeviceAsync(
            new CreateDeviceRequest("Managed device", 20, deviceType.Id),
            CancellationToken.None);

        Assert.Equal("Managed device", device.Name);
        Assert.Equal(20, device.SlaveAddress);
        Assert.True(device.IsEnabled);
    }

    [Fact]
    public async Task CreateDeviceAsync_DuplicateSlaveAddressIsRejected()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var deviceType = await AddDeviceTypeAsync(dbContext);
        var service = new DeviceManagementService(dbContext);
        await service.CreateDeviceAsync(
            new CreateDeviceRequest("First device", 21, deviceType.Id),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateDeviceAsync(
                new CreateDeviceRequest("Second device", 21, deviceType.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateRegisterDefinitionAsync_DuplicateAddressIsRejected()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var deviceType = await AddDeviceTypeAsync(dbContext);
        var service = new DeviceManagementService(dbContext);
        var request = new CreateRegisterDefinitionRequest(
            deviceType.Id,
            7000,
            "Status",
            "ReadOnly",
            null,
            0,
            1,
            null,
            0);

        await service.CreateRegisterDefinitionAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateRegisterDefinitionAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateRegisterDefinitionAsync_InitializesValuesForExistingDevices()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var deviceType = await AddDeviceTypeAsync(dbContext);
        var service = new DeviceManagementService(dbContext);
        await service.CreateDeviceAsync(
            new CreateDeviceRequest("Device A", 22, deviceType.Id),
            CancellationToken.None);
        await service.CreateDeviceAsync(
            new CreateDeviceRequest("Device B", 23, deviceType.Id),
            CancellationToken.None);

        var register = await service.CreateRegisterDefinitionAsync(
            new CreateRegisterDefinitionRequest(
                deviceType.Id,
                7001,
                "Temperature",
                "ReadWrite",
                "C",
                -40,
                160,
                null,
                25),
            CancellationToken.None);

        var valueCount = await dbContext.RegisterValues.CountAsync(
            value => value.RegisterDefinitionId == register.Id);

        Assert.Equal(2, valueCount);
    }

    private static async Task<DeviceType> AddDeviceTypeAsync(
        ModbusLab.Infrastructure.Persistence.ModbusLabDbContext dbContext)
    {
        var deviceType = new DeviceType($"ManagedType-{Guid.NewGuid()}", "Managed test type");
        await dbContext.DeviceTypes.AddAsync(deviceType);
        await dbContext.SaveChangesAsync();
        return deviceType;
    }
}
