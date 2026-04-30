using ModbusLab.Domain.Devices;
using ModbusLab.Domain.Logs;
using ModbusLab.Domain.Registers;

namespace ModbusLab.Infrastructure.InMemory;

public sealed class InMemoryStore
{
    private readonly object _syncRoot = new();

    internal object SyncRoot => _syncRoot;

    internal List<DeviceType> DeviceTypes { get; } = new();

    internal List<SlaveDevice> Devices { get; } = new();

    internal List<RegisterDefinition> RegisterDefinitions { get; } = new();

    internal List<RegisterValue> RegisterValues { get; } = new();

    internal List<ModbusLogEntry> Logs { get; } = new();

    public InMemoryStore()
    {
        Seed();
    }

    private void Seed()
    {
        var standRps = new DeviceType(
            "StandRps",
            "Power and control test stand simulator");

        DeviceTypes.Add(standRps);

        var slaveDevice = new SlaveDevice(
            "Stand RPS #1",
            slaveAddress: 1,
            deviceTypeId: standRps.Id);

        Devices.Add(slaveDevice);

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

        RegisterDefinitions.AddRange(registers);

        AddInitialValue(slaveDevice.Id, registers[0].Id, 0);
        AddInitialValue(slaveDevice.Id, registers[1].Id, 0);
        AddInitialValue(slaveDevice.Id, registers[2].Id, 12000);
        AddInitialValue(slaveDevice.Id, registers[3].Id, 250);
        AddInitialValue(slaveDevice.Id, registers[4].Id, 0);
    }

    private void AddInitialValue(Guid slaveDeviceId, Guid registerDefinitionId, int value)
    {
        RegisterValues.Add(new RegisterValue(
            slaveDeviceId,
            registerDefinitionId,
            value));
    }
}