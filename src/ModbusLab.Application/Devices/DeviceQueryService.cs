using ModbusLab.Application.Abstractions;
using ModbusLab.Application.Registers;

namespace ModbusLab.Application.Devices;

public sealed class DeviceQueryService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IRegisterRepository _registerRepository;

    public DeviceQueryService(
        IDeviceRepository deviceRepository,
        IRegisterRepository registerRepository)
    {
        _deviceRepository = deviceRepository;
        _registerRepository = registerRepository;
    }

    public async Task<IReadOnlyList<DeviceDto>> GetDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        var devices = await _deviceRepository.GetAllAsync(cancellationToken);

        return devices
            .Select(device => new DeviceDto(
                device.Id,
                device.Name,
                device.SlaveAddress,
                device.DeviceTypeId,
                device.IsEnabled))
            .ToList();
    }

    public async Task<IReadOnlyList<RegisterDto>> GetDeviceRegistersAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdAsync(deviceId, cancellationToken);

        if (device is null)
            return Array.Empty<RegisterDto>();

        var definitions = await _registerRepository.GetDefinitionsByDeviceTypeIdAsync(
            device.DeviceTypeId,
            cancellationToken);

        var result = new List<RegisterDto>();

        foreach (var definition in definitions.OrderBy(register => register.Address))
        {
            var value = await _registerRepository.GetValueAsync(
                device.Id,
                definition.Id,
                cancellationToken);

            result.Add(new RegisterDto(
                definition.Id,
                definition.Address,
                definition.Name,
                definition.AccessMode.ToString(),
                definition.Unit,
                definition.MinValue,
                definition.MaxValue,
                value?.Value,
                value?.UpdatedAtUtc));
        }

        return result;
    }
}
