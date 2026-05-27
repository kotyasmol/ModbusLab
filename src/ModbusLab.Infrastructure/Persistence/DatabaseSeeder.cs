using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        await SeedDevicesAsync(dbContext);
        await SeedTestProfilesAsync(dbContext);
        await SeedUsersAsync(dbContext);
    }

    private static async Task SeedDevicesAsync(ModbusLabDbContext dbContext)
    {
        await EnsureDemoDeviceAsync(
            dbContext,
            "StandRps",
            "Power and control test stand simulator",
            "Stand RPS #1",
            1,
            new DemoRegister(1300, "Power control", RegisterAccessMode.ReadWrite, null, 0, 1, "0 - power off, 1 - power on", 0),
            new DemoRegister(1301, "Test mode", RegisterAccessMode.ReadWrite, null, 0, 10, "Current test mode", 0),
            new DemoRegister(1305, "Output voltage", RegisterAccessMode.ReadOnly, "mV", 11000, 13000, "Measured output voltage", 12000),
            new DemoRegister(1306, "Output current", RegisterAccessMode.ReadOnly, "mA", 0, 3000, "Measured output current", 250),
            new DemoRegister(1310, "Error code", RegisterAccessMode.ReadOnly, null, 0, 999, "Current device error code", 0));

        await EnsureDemoDeviceAsync(
            dbContext,
            "CoolantPump",
            "Cooling circuit pump controller simulator",
            "Coolant pump #1",
            2,
            new DemoRegister(2000, "Pump enable", RegisterAccessMode.ReadWrite, null, 0, 1, "0 - stopped, 1 - running", 0),
            new DemoRegister(2001, "Target speed", RegisterAccessMode.ReadWrite, "%", 0, 100, "Requested pump speed", 40),
            new DemoRegister(2005, "Line pressure", RegisterAccessMode.ReadOnly, "kPa", 100, 400, "Measured coolant pressure", 260),
            new DemoRegister(2006, "Fluid temperature", RegisterAccessMode.ReadOnly, "C", 10, 90, "Measured coolant temperature", 42),
            new DemoRegister(2010, "Alarm code", RegisterAccessMode.ReadOnly, null, 0, 999, "Pump alarm code", 0));

        await EnsureDemoDeviceAsync(
            dbContext,
            "ThermalChamber",
            "Temperature chamber controller simulator",
            "Thermal chamber #1",
            3,
            new DemoRegister(3000, "Heater enable", RegisterAccessMode.ReadWrite, null, 0, 1, "0 - off, 1 - on", 0),
            new DemoRegister(3001, "Target temperature", RegisterAccessMode.ReadWrite, "C", -40, 160, "Requested chamber temperature", 25),
            new DemoRegister(3005, "Actual temperature", RegisterAccessMode.ReadOnly, "C", -45, 170, "Measured chamber temperature", 72),
            new DemoRegister(3006, "Humidity", RegisterAccessMode.ReadOnly, "%", 0, 100, "Measured humidity", 35),
            new DemoRegister(3010, "Door state", RegisterAccessMode.ReadOnly, null, 0, 1, "0 - closed, 1 - open", 0));

        await EnsureDemoDeviceAsync(
            dbContext,
            "ConveyorDrive",
            "Conveyor belt drive simulator",
            "Conveyor drive #1",
            4,
            new DemoRegister(4000, "Drive enable", RegisterAccessMode.ReadWrite, null, 0, 1, "0 - stopped, 1 - running", 0),
            new DemoRegister(4001, "Belt speed", RegisterAccessMode.ReadWrite, "%", 0, 100, "Requested belt speed", 35),
            new DemoRegister(4005, "Motor load", RegisterAccessMode.ReadOnly, "%", 0, 120, "Measured motor load", 45),
            new DemoRegister(4006, "Jam sensor", RegisterAccessMode.ReadOnly, null, 0, 1, "0 - clear, 1 - jammed", 0),
            new DemoRegister(4010, "Items per minute", RegisterAccessMode.ReadOnly, "pcs/min", 0, 600, "Measured throughput", 210));

        await EnsureDemoDeviceAsync(
            dbContext,
            "AirCompressor",
            "Compressed air station simulator",
            "Air compressor #1",
            5,
            new DemoRegister(5000, "Compressor enable", RegisterAccessMode.ReadWrite, null, 0, 1, "0 - off, 1 - on", 0),
            new DemoRegister(5001, "Pressure setpoint", RegisterAccessMode.ReadWrite, "kPa", 500, 900, "Requested air pressure", 750),
            new DemoRegister(5005, "Tank pressure", RegisterAccessMode.ReadOnly, "kPa", 0, 1000, "Measured tank pressure", 750),
            new DemoRegister(5006, "Oil temperature", RegisterAccessMode.ReadOnly, "C", 0, 120, "Measured oil temperature", 68),
            new DemoRegister(5010, "Service hours", RegisterAccessMode.ReadOnly, "h", 0, 100000, "Accumulated service hours", 1840));

        await EnsureDemoDeviceAsync(
            dbContext,
            "ClimateUnit",
            "Cabinet climate control simulator",
            "Climate unit #1",
            6,
            new DemoRegister(6000, "Cooling enable", RegisterAccessMode.ReadWrite, null, 0, 1, "0 - off, 1 - on", 0),
            new DemoRegister(6001, "Fan speed", RegisterAccessMode.ReadWrite, "%", 0, 100, "Requested fan speed", 55),
            new DemoRegister(6005, "Cabinet temperature", RegisterAccessMode.ReadOnly, "C", -20, 80, "Measured cabinet temperature", -5),
            new DemoRegister(6006, "Cabinet humidity", RegisterAccessMode.ReadOnly, "%", 0, 100, "Measured cabinet humidity", 50),
            new DemoRegister(6010, "Filter status", RegisterAccessMode.ReadOnly, "%", 0, 100, "Remaining filter resource", 82));
    }

    private static async Task SeedTestProfilesAsync(ModbusLabDbContext dbContext)
    {
        await EnsureTestProfileAsync(
            dbContext,
            "Stand RPS smoke test",
            "Basic stand scenario: power on, wait, verify voltage and current.",
            StepSpec.Write(1, "Enable stand power", 1, 1300, 1),
            StepSpec.Delay(2, "Wait for measurements to stabilize", 1500),
            StepSpec.Check(3, "Check output voltage", 1, 1305, 11700, 12300),
            StepSpec.Check(4, "Check output current", 1, 1306, 0, 800));

        await EnsureTestProfileAsync(
            dbContext,
            "Coolant pump startup",
            "Passing pump scenario with a short stabilization period.",
            StepSpec.Write(1, "Start pump", 2, 2000, 1),
            StepSpec.Write(2, "Set pump speed", 2, 2001, 65),
            StepSpec.Delay(3, "Wait for flow", 700),
            StepSpec.Check(4, "Check coolant pressure", 2, 2005, 220, 300));

        await EnsureTestProfileAsync(
            dbContext,
            "Coolant pump overpressure check",
            "Failing scenario: expected pressure range is intentionally too high.",
            StepSpec.Write(1, "Start pump", 2, 2000, 1),
            StepSpec.Delay(2, "Wait before pressure check", 1800),
            StepSpec.Check(3, "Expect high pressure", 2, 2005, 350, 380));

        await EnsureTestProfileAsync(
            dbContext,
            "Thermal chamber warmup",
            "Passing chamber scenario with medium wait time.",
            StepSpec.Write(1, "Enable heater", 3, 3000, 1),
            StepSpec.Write(2, "Set target temperature", 3, 3001, 75),
            StepSpec.Delay(3, "Wait for chamber response", 2400),
            StepSpec.Check(4, "Check chamber temperature", 3, 3005, 60, 90));

        await EnsureTestProfileAsync(
            dbContext,
            "Thermal chamber low-temperature guard",
            "Failing scenario: measured temperature is warmer than the expected cold range.",
            StepSpec.Delay(1, "Short cold-soak wait", 500),
            StepSpec.Check(2, "Expect cold chamber", 3, 3005, 20, 40));

        await EnsureTestProfileAsync(
            dbContext,
            "Conveyor nominal run",
            "Passing conveyor scenario with load and throughput checks.",
            StepSpec.Write(1, "Start conveyor", 4, 4000, 1),
            StepSpec.Write(2, "Set belt speed", 4, 4001, 70),
            StepSpec.Delay(3, "Wait for belt to settle", 1100),
            StepSpec.Check(4, "Check motor load", 4, 4005, 30, 80),
            StepSpec.Check(5, "Check throughput", 4, 4010, 180, 260));

        await EnsureTestProfileAsync(
            dbContext,
            "Conveyor invalid speed rejection",
            "Failing scenario: write command uses a value outside the register range.",
            StepSpec.Delay(1, "Operator confirmation delay", 900),
            StepSpec.Write(2, "Set impossible belt speed", 4, 4001, 120));

        await EnsureTestProfileAsync(
            dbContext,
            "Air compressor pressure build",
            "Passing compressor scenario with the longest stabilization wait.",
            StepSpec.Write(1, "Start compressor", 5, 5000, 1),
            StepSpec.Write(2, "Set pressure target", 5, 5001, 750),
            StepSpec.Delay(3, "Wait for tank pressure", 3000),
            StepSpec.Check(4, "Check tank pressure", 5, 5005, 720, 780));

        await EnsureTestProfileAsync(
            dbContext,
            "Air compressor leak check",
            "Failing scenario: required pressure is intentionally above current value.",
            StepSpec.Delay(1, "Wait during leak simulation", 1600),
            StepSpec.Check(2, "Expect pressure hold", 5, 5005, 800, 900));

        await EnsureTestProfileAsync(
            dbContext,
            "Climate unit cooling",
            "Passing cooling scenario with humidity and temperature checks.",
            StepSpec.Write(1, "Enable cooling", 6, 6000, 1),
            StepSpec.Write(2, "Set fan speed", 6, 6001, 60),
            StepSpec.Delay(3, "Wait for cabinet airflow", 2200),
            StepSpec.Check(4, "Check cabinet temperature", 6, 6005, -10, 5),
            StepSpec.Check(5, "Check cabinet humidity", 6, 6006, 40, 60));

        await EnsureTestProfileAsync(
            dbContext,
            "Climate unit humidity alarm",
            "Failing scenario: humidity expectation is intentionally too strict.",
            StepSpec.Delay(1, "Long humidity sampling window", 4000),
            StepSpec.Check(2, "Expect high humidity", 6, 6006, 70, 85));
    }

    private static async Task EnsureDemoDeviceAsync(
        ModbusLabDbContext dbContext,
        string typeName,
        string typeDescription,
        string deviceName,
        int slaveAddress,
        params DemoRegister[] registers)
    {
        var deviceType = await dbContext.DeviceTypes
            .FirstOrDefaultAsync(type => type.Name == typeName);

        if (deviceType is null)
        {
            deviceType = new DeviceType(typeName, typeDescription);
            await dbContext.DeviceTypes.AddAsync(deviceType);
        }

        var slaveDevice = await dbContext.SlaveDevices
            .FirstOrDefaultAsync(device => device.SlaveAddress == slaveAddress);

        if (slaveDevice is null)
        {
            slaveDevice = new SlaveDevice(deviceName, slaveAddress, deviceType.Id);
            await dbContext.SlaveDevices.AddAsync(slaveDevice);
        }

        foreach (var register in registers)
        {
            var definition = await dbContext.RegisterDefinitions.FirstOrDefaultAsync(existing =>
                existing.DeviceTypeId == deviceType.Id &&
                existing.Address == register.Address);

            if (definition is null)
            {
                definition = new RegisterDefinition(
                    deviceType.Id,
                    register.Address,
                    register.Name,
                    register.AccessMode,
                    register.Unit,
                    register.MinValue,
                    register.MaxValue,
                    register.Description);

                await dbContext.RegisterDefinitions.AddAsync(definition);
            }

            var hasValue = await dbContext.RegisterValues.AnyAsync(value =>
                value.SlaveDeviceId == slaveDevice.Id &&
                value.RegisterDefinitionId == definition.Id);

            if (!hasValue)
            {
                await dbContext.RegisterValues.AddAsync(
                    new RegisterValue(slaveDevice.Id, definition.Id, register.InitialValue));
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureTestProfileAsync(
        ModbusLabDbContext dbContext,
        string name,
        string description,
        params StepSpec[] steps)
    {
        if (await dbContext.TestProfiles.AnyAsync(profile => profile.Name == name))
            return;

        var profile = new TestProfile(name, description);
        await dbContext.TestProfiles.AddAsync(profile);

        foreach (var step in steps)
        {
            var profileStep = step.Type switch
            {
                TestStepType.WriteRegister => TestStep.CreateWriteRegister(
                    profile.Id,
                    step.OrderIndex,
                    step.Name,
                    step.SlaveAddress!.Value,
                    step.RegisterAddress!.Value,
                    step.Value!.Value),

                TestStepType.Delay => TestStep.CreateDelay(
                    profile.Id,
                    step.OrderIndex,
                    step.Name,
                    step.DelayMs!.Value),

                TestStepType.CheckRegisterRange => TestStep.CreateCheckRegisterRange(
                    profile.Id,
                    step.OrderIndex,
                    step.Name,
                    step.SlaveAddress!.Value,
                    step.RegisterAddress!.Value,
                    step.MinValue!.Value,
                    step.MaxValue!.Value),

                _ => throw new InvalidOperationException($"Unsupported test step type: {step.Type}.")
            };

            await dbContext.TestSteps.AddAsync(profileStep);
        }

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

    private sealed record DemoRegister(
        int Address,
        string Name,
        RegisterAccessMode AccessMode,
        string? Unit,
        int? MinValue,
        int? MaxValue,
        string? Description,
        int InitialValue);

    private sealed record StepSpec(
        TestStepType Type,
        int OrderIndex,
        string Name,
        int? SlaveAddress = null,
        int? RegisterAddress = null,
        int? Value = null,
        int? MinValue = null,
        int? MaxValue = null,
        int? DelayMs = null)
    {
        public static StepSpec Write(
            int orderIndex,
            string name,
            int slaveAddress,
            int registerAddress,
            int value)
        {
            return new StepSpec(
                TestStepType.WriteRegister,
                orderIndex,
                name,
                slaveAddress,
                registerAddress,
                value);
        }

        public static StepSpec Delay(
            int orderIndex,
            string name,
            int delayMs)
        {
            return new StepSpec(
                TestStepType.Delay,
                orderIndex,
                name,
                DelayMs: delayMs);
        }

        public static StepSpec Check(
            int orderIndex,
            string name,
            int slaveAddress,
            int registerAddress,
            int minValue,
            int maxValue)
        {
            return new StepSpec(
                TestStepType.CheckRegisterRange,
                orderIndex,
                name,
                slaveAddress,
                registerAddress,
                MinValue: minValue,
                MaxValue: maxValue);
        }
    }
}
