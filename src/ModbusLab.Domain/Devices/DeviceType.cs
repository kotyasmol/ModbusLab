namespace ModbusLab.Domain.Devices;

public sealed class DeviceType
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    private DeviceType()
    {
        Name = string.Empty;
    }

    public DeviceType(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device type name cannot be empty.", nameof(name));

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device type name cannot be empty.", nameof(name));

        Name = name;
    }

    public void ChangeDescription(string? description)
    {
        Description = description;
    }
}