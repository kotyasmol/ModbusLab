namespace ModbusLab.Domain.Registers;

public sealed class RegisterDefinition
{
    public Guid Id { get; private set; }

    public Guid DeviceTypeId { get; private set; }

    public int Address { get; private set; }

    public string Name { get; private set; }

    public RegisterAccessMode AccessMode { get; private set; }

    public string? Unit { get; private set; }

    public int? MinValue { get; private set; }

    public int? MaxValue { get; private set; }

    public string? Description { get; private set; }

    private RegisterDefinition()
    {
        Name = string.Empty;
    }

    public RegisterDefinition(
        Guid deviceTypeId,
        int address,
        string name,
        RegisterAccessMode accessMode,
        string? unit = null,
        int? minValue = null,
        int? maxValue = null,
        string? description = null)
    {
        if (deviceTypeId == Guid.Empty)
            throw new ArgumentException("Device type id cannot be empty.", nameof(deviceTypeId));

        if (address < 0)
            throw new ArgumentOutOfRangeException(nameof(address), "Register address cannot be negative.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Register name cannot be empty.", nameof(name));

        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
            throw new ArgumentException("Min value cannot be greater than max value.");

        Id = Guid.NewGuid();
        DeviceTypeId = deviceTypeId;
        Address = address;
        Name = name;
        AccessMode = accessMode;
        Unit = unit;
        MinValue = minValue;
        MaxValue = maxValue;
        Description = description;
    }

    public bool CanWrite()
    {
        return AccessMode == RegisterAccessMode.ReadWrite;
    }

    public bool IsValueInRange(int value)
    {
        if (MinValue.HasValue && value < MinValue.Value)
            return false;

        if (MaxValue.HasValue && value > MaxValue.Value)
            return false;

        return true;
    }
}
