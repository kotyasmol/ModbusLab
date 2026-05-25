using ModbusLab.Domain.Modbus;

namespace ModbusLab.Domain.Logs;

public sealed class ModbusLogEntry
{
    public Guid Id { get; private set; }

    public DateTime TimestampUtc { get; private set; }

    public int SlaveAddress { get; private set; }

    public ModbusFunctionCode FunctionCode { get; private set; }

    public int RegisterAddress { get; private set; }

    public int? Value { get; private set; }

    public ModbusOperationStatus Status { get; private set; }

    public string Message { get; private set; }

    public Guid? SlaveDeviceId { get; private set; }

    private ModbusLogEntry()
    {
        Message = string.Empty;
    }

    public ModbusLogEntry(
        int slaveAddress,
        ModbusFunctionCode functionCode,
        int registerAddress,
        ModbusOperationStatus status,
        string message,
        Guid? slaveDeviceId = null,
        int? value = null)
    {
        if (slaveAddress < 1 || slaveAddress > 247)
            throw new ArgumentOutOfRangeException(nameof(slaveAddress), "Modbus slave address must be between 1 and 247.");

        if (registerAddress < 0)
            throw new ArgumentOutOfRangeException(nameof(registerAddress), "Register address cannot be negative.");

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Log message cannot be empty.", nameof(message));

        Id = Guid.NewGuid();
        TimestampUtc = DateTime.UtcNow;
        SlaveAddress = slaveAddress;
        FunctionCode = functionCode;
        RegisterAddress = registerAddress;
        Status = status;
        Message = message;
        SlaveDeviceId = slaveDeviceId;
        Value = value;
    }
}
