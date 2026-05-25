using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModbusLab.Domain.Devices;
using ModbusLab.Domain.Registers;
using ModbusLab.Domain.Testing;

namespace ModbusLab.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ModbusLabDbContext>();

        await dbContext.Database.MigrateAsync();

        if (!await dbContext.DeviceTypes.AnyAsync())
        {
            await SeedDevicesAsync(dbContext);
        }

        if (!await dbContext.TestProfiles.AnyAsync())
        {
            await SeedTestProfilesAsync(dbContext);
        }
    }

    private static async Task SeedDevicesAsync(ModbusLabDbContext dbContext)
    {
        var standRps = new DeviceType(
            "StandRps",
            "Power and control test stand simulator");

        await dbContext.DeviceTypes.AddAsync(standRps);

        var slaveDevice = new SlaveDevice(
            "Stand RPS #1",
            slaveAddress: 1,
            deviceTypeId: standRps.Id);

        await dbContext.SlaveDevices.AddAsync(slaveDevice);

        var registers = new[]
        {
            new RegisterDefinition(
                standRps.Id,
                address: 1300,
                name: "Power control",
                accessMode: RegisterAccessMode.ReadWrite,
                unit: null,
                minValue: 0,
                maxValue: 1,
                description: "0 - power off, 1 - power on"),

            new RegisterDefinition(
                standRps.Id,
                address: 1301,
                name: "Test mode",
                accessMode: RegisterAccessMode.ReadWrite,
                unit: null,
                minValue: 0,
                maxValue: 10,
                description: "Current test mode"),

            new RegisterDefinition(
                standRps.Id,
                address: 1305,
                name: "Output voltage",
                accessMode: RegisterAccessMode.ReadOnly,
                unit: "mV",
                minValue: 11000,
                maxValue: 13000,
                description: "Measured output voltage"),

            new RegisterDefinition(
                standRps.Id,
                address: 1306,
                name: "Output current",
                accessMode: RegisterAccessMode.ReadOnly,
                unit: "mA",
                minValue: 0,
                maxValue: 3000,
                description: "Measured output current"),

            new RegisterDefinition(
                standRps.Id,
                address: 1310,
                name: "Error code",
                accessMode: RegisterAccessMode.ReadOnly,
                unit: null,
                minValue: 0,
                maxValue: 999,
                description: "Current device error code")
        };

        await dbContext.RegisterDefinitions.AddRangeAsync(registers);

        await dbContext.RegisterValues.AddRangeAsync(
            new RegisterValue(slaveDevice.Id, registers[0].Id, 0),
            new RegisterValue(slaveDevice.Id, registers[1].Id, 0),
            new RegisterValue(slaveDevice.Id, registers[2].Id, 12000),
            new RegisterValue(slaveDevice.Id, registers[3].Id, 250),
            new RegisterValue(slaveDevice.Id, registers[4].Id, 0));

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedTestProfilesAsync(ModbusLabDbContext dbContext)
    {
        var standDevice = await dbContext.SlaveDevices
            .OrderBy(device => device.SlaveAddress)
            .FirstOrDefaultAsync();

        if (standDevice is null)
            return;

        var profile = new TestProfile(
            "Stand RPS smoke test",
            "Базовый сценарий: включить питание, подождать, проверить напряжение и ток.");

        await dbContext.TestProfiles.AddAsync(profile);

        await dbContext.TestSteps.AddRangeAsync(
            TestStep.CreateWriteRegister(
                profile.Id,
                orderIndex: 1,
                name: "Включить питание стенда",
                slaveAddress: standDevice.SlaveAddress,
                registerAddress: 1300,
                value: 1),

            TestStep.CreateDelay(
                profile.Id,
                orderIndex: 2,
                name: "Подождать стабилизацию измерений",
                delayMs: 1500),

            TestStep.CreateCheckRegisterRange(
                profile.Id,
                orderIndex: 3,
                name: "Проверить выходное напряжение",
                slaveAddress: standDevice.SlaveAddress,
                registerAddress: 1305,
                minValue: 11700,
                maxValue: 12300),

            TestStep.CreateCheckRegisterRange(
                profile.Id,
                orderIndex: 4,
                name: "Проверить выходной ток",
                slaveAddress: standDevice.SlaveAddress,
                registerAddress: 1306,
                minValue: 0,
                maxValue: 800));

        await dbContext.SaveChangesAsync();
    }
}
