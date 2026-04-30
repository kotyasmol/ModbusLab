namespace ModbusLab.Domain.Registers;

public sealed class RegisterValue
{
    public Guid Id { get; private set; }

    public Guid SlaveDeviceId { get; private set; }

    public Guid RegisterDefinitionId { get; private set; }

    public int Value { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    private RegisterValue()
    {
    }

    public RegisterValue(Guid slaveDeviceId, Guid registerDefinitionId, int initialValue = 0)
    {
        if (slaveDeviceId == Guid.Empty)
            throw new ArgumentException("Slave device id cannot be empty.", nameof(slaveDeviceId));

        if (registerDefinitionId == Guid.Empty)
            throw new ArgumentException("Register definition id cannot be empty.", nameof(registerDefinitionId));

        Id = Guid.NewGuid();
        SlaveDeviceId = slaveDeviceId;
        RegisterDefinitionId = registerDefinitionId;
        Value = initialValue;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateValue(int value)
    {
        Value = value;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}