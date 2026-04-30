namespace ModbusLab.Application.Devices;

public sealed record DeviceDto(
    Guid Id,
    string Name,
    int SlaveAddress,
    Guid DeviceTypeId,
    bool IsEnabled);