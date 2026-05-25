namespace ModbusLab.Domain.Devices;

public sealed class SlaveDevice
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int SlaveAddress { get; private set; }

    public Guid DeviceTypeId { get; private set; }

    public bool IsEnabled { get; private set; }

    private SlaveDevice()
    {
        Name = string.Empty;
    }

    public SlaveDevice(string name, int slaveAddress, Guid deviceTypeId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Slave device name cannot be empty.", nameof(name));

        if (slaveAddress < 1 || slaveAddress > 247)
            throw new ArgumentOutOfRangeException(nameof(slaveAddress), "Modbus slave address must be between 1 and 247.");

        if (deviceTypeId == Guid.Empty)
            throw new ArgumentException("Device type id cannot be empty.", nameof(deviceTypeId));

        Id = Guid.NewGuid();
        Name = name;
        SlaveAddress = slaveAddress;
        DeviceTypeId = deviceTypeId;
        IsEnabled = true;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Slave device name cannot be empty.", nameof(name));

        Name = name;
    }

    public void ChangeSlaveAddress(int slaveAddress)
    {
        if (slaveAddress < 1 || slaveAddress > 247)
            throw new ArgumentOutOfRangeException(nameof(slaveAddress), "Modbus slave address must be between 1 and 247.");

        SlaveAddress = slaveAddress;
    }
}
