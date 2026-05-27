using Microsoft.EntityFrameworkCore;
using ModbusLab.Domain.Devices;
using ModbusLab.Domain.Registers;
using ModbusLab.Infrastructure.Persistence;

namespace ModbusLab.Api.Devices;

public sealed class DeviceManagementService
{
    private readonly ModbusLabDbContext _dbContext;

    public DeviceManagementService(ModbusLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<DeviceTypeDto>> GetDeviceTypesAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.DeviceTypes
            .AsNoTracking()
            .OrderBy(type => type.Name)
            .Select(type => new DeviceTypeDto(type.Id, type.Name, type.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceTypeDto> CreateDeviceTypeAsync(
        CreateDeviceTypeRequest request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, "Device type name is required.");

        if (await _dbContext.DeviceTypes.AnyAsync(
                type => type.Name.ToLower() == name.ToLower(),
                cancellationToken))
        {
            throw new InvalidOperationException("Device type name is already taken.");
        }

        var deviceType = new DeviceType(name, NormalizeOptional(request.Description));

        await _dbContext.DeviceTypes.AddAsync(deviceType, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeviceTypeDto(deviceType.Id, deviceType.Name, deviceType.Description);
    }

    public async Task<ManagedDeviceDto> CreateDeviceAsync(
        CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, "Device name is required.");

        if (!await _dbContext.DeviceTypes.AnyAsync(
                type => type.Id == request.DeviceTypeId,
                cancellationToken))
        {
            throw new ArgumentException("Device type not found.");
        }

        if (await _dbContext.SlaveDevices.AnyAsync(
                device => device.SlaveAddress == request.SlaveAddress,
                cancellationToken))
        {
            throw new InvalidOperationException("Slave address is already used.");
        }

        var device = new SlaveDevice(name, request.SlaveAddress, request.DeviceTypeId);

        await _dbContext.SlaveDevices.AddAsync(device, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(device);
    }

    public async Task<ManagedDeviceDto?> ChangeDeviceStatusAsync(
        Guid deviceId,
        ChangeDeviceStatusRequest request,
        CancellationToken cancellationToken)
    {
        var device = await _dbContext.SlaveDevices
            .FirstOrDefaultAsync(candidate => candidate.Id == deviceId, cancellationToken);

        if (device is null)
            return null;

        if (request.IsEnabled)
        {
            device.Enable();
        }
        else
        {
            device.Disable();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(device);
    }

    public async Task<RegisterDefinitionDto> CreateRegisterDefinitionAsync(
        CreateRegisterDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Name, "Register name is required.");
        var accessMode = ParseAccessMode(request.AccessMode);

        if (!await _dbContext.DeviceTypes.AnyAsync(
                type => type.Id == request.DeviceTypeId,
                cancellationToken))
        {
            throw new ArgumentException("Device type not found.");
        }

        if (await _dbContext.RegisterDefinitions.AnyAsync(
                definition =>
                    definition.DeviceTypeId == request.DeviceTypeId &&
                    definition.Address == request.Address,
                cancellationToken))
        {
            throw new InvalidOperationException("Register address is already used for this device type.");
        }

        var definition = new RegisterDefinition(
            request.DeviceTypeId,
            request.Address,
            name,
            accessMode,
            NormalizeOptional(request.Unit),
            request.MinValue,
            request.MaxValue,
            NormalizeOptional(request.Description));

        await _dbContext.RegisterDefinitions.AddAsync(definition, cancellationToken);

        var devices = await _dbContext.SlaveDevices
            .Where(device => device.DeviceTypeId == request.DeviceTypeId)
            .ToListAsync(cancellationToken);

        foreach (var device in devices)
        {
            await _dbContext.RegisterValues.AddAsync(
                new RegisterValue(device.Id, definition.Id, request.InitialValue ?? 0),
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(definition);
    }

    private static RegisterAccessMode ParseAccessMode(string accessMode)
    {
        if (Enum.TryParse<RegisterAccessMode>(accessMode, ignoreCase: false, out var parsed))
            return parsed;

        throw new ArgumentException("Access mode must be ReadOnly or ReadWrite.");
    }

    private static ManagedDeviceDto ToDto(SlaveDevice device)
    {
        return new ManagedDeviceDto(
            device.Id,
            device.Name,
            device.SlaveAddress,
            device.DeviceTypeId,
            device.IsEnabled);
    }

    private static RegisterDefinitionDto ToDto(RegisterDefinition definition)
    {
        return new RegisterDefinitionDto(
            definition.Id,
            definition.DeviceTypeId,
            definition.Address,
            definition.Name,
            definition.AccessMode.ToString(),
            definition.Unit,
            definition.MinValue,
            definition.MaxValue,
            definition.Description);
    }

    private static string NormalizeRequired(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(message);

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public sealed record DeviceTypeDto(Guid Id, string Name, string? Description);

public sealed record ManagedDeviceDto(
    Guid Id,
    string Name,
    int SlaveAddress,
    Guid DeviceTypeId,
    bool IsEnabled);

public sealed record RegisterDefinitionDto(
    Guid Id,
    Guid DeviceTypeId,
    int Address,
    string Name,
    string AccessMode,
    string? Unit,
    int? MinValue,
    int? MaxValue,
    string? Description);

public sealed record CreateDeviceTypeRequest(string Name, string? Description);

public sealed record CreateDeviceRequest(string Name, int SlaveAddress, Guid DeviceTypeId);

public sealed record ChangeDeviceStatusRequest(bool IsEnabled);

public sealed record CreateRegisterDefinitionRequest(
    Guid DeviceTypeId,
    int Address,
    string Name,
    string AccessMode,
    string? Unit,
    int? MinValue,
    int? MaxValue,
    string? Description,
    int? InitialValue);
