using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ModbusLab.Domain.Devices;
using ModbusLab.Domain.Registers;
using ModbusLab.Domain.Testing;
using ModbusLab.Domain.Users;

namespace ModbusLab.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ModbusLabDbContext>();

        await BaselineLegacyEnsureCreatedDatabaseAsync(dbContext);
        await dbContext.Database.MigrateAsync();

        if (!await dbContext.DeviceTypes.AnyAsync())
        {
            await SeedDevicesAsync(dbContext);
        }

        if (!await dbContext.TestProfiles.AnyAsync())
        {
            await SeedTestProfilesAsync(dbContext);
        }

        await SeedUsersAsync(dbContext);
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

    private static async Task SeedUsersAsync(ModbusLabDbContext dbContext)
    {
        await EnsureDemoUserAsync(
            dbContext,
            userName: "admin",
            email: "admin@modbuslab.local",
            password: "Admin123!",
            role: "Admin");

        await EnsureDemoUserAsync(
            dbContext,
            userName: "engineer",
            email: "engineer@modbuslab.local",
            password: "Engineer123!",
            role: "Engineer");

        await EnsureDemoUserAsync(
            dbContext,
            userName: "viewer",
            email: "viewer@modbuslab.local",
            password: "Viewer123!",
            role: "Viewer");

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureDemoUserAsync(
        ModbusLabDbContext dbContext,
        string userName,
        string email,
        string password,
        string role)
    {
        if (await dbContext.AppUsers.AnyAsync(user => user.UserName == userName))
            return;

        var user = new AppUser(
            userName,
            email,
            passwordHash: "__pending__",
            role);

        var passwordHasher = new PasswordHasher<AppUser>();
        user.SetPasswordHash(passwordHasher.HashPassword(user, password));

        await dbContext.AppUsers.AddAsync(user);
    }

    private static async Task BaselineLegacyEnsureCreatedDatabaseAsync(ModbusLabDbContext dbContext)
    {
        var hasExistingSchema = await TableExistsAsync(dbContext, "device_types");

        if (!hasExistingSchema)
            return;

        await dbContext.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            """);

        await AddBaselineMigrationAsync(dbContext, "20260430061940_InitialCreate");
        await AddBaselineMigrationAsync(dbContext, "20260525110010_AddTestingModule");

        if (await TableExistsAsync(dbContext, "app_users"))
        {
            await AddBaselineMigrationAsync(dbContext, "20260526094216_AddAppUsers");
        }

        if (await TableExistsAsync(dbContext, "audit_logs"))
        {
            await AddBaselineMigrationAsync(dbContext, "20260526103619_AddAuditLogs");
        }
    }

    private static async Task AddBaselineMigrationAsync(
        ModbusLabDbContext dbContext,
        string migrationId)
    {
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ({migrationId}, '10.0.7')
            ON CONFLICT ("MigrationId") DO NOTHING;
            """);
    }

    private static async Task<bool> TableExistsAsync(
        ModbusLabDbContext dbContext,
        string tableName)
    {
        var result = await ExecuteScalarAsync(
            dbContext,
            """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name = @tableName
            );
            """,
            tableName);

        return result is true;
    }

    private static async Task<object?> ExecuteScalarAsync(
        ModbusLabDbContext dbContext,
        string commandText,
        string? tableName = null)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;

            if (tableName is not null)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = "tableName";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);
            }

            return await command.ExecuteScalarAsync();
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
