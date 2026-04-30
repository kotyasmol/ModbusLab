using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Logs;
using ModbusLab.Domain.Modbus;
using ModbusLab.Domain.Registers;

namespace ModbusLab.Application.Modbus;

public sealed class ModbusRegisterService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IRegisterRepository _registerRepository;
    private readonly IModbusLogRepository _logRepository;

    public ModbusRegisterService(
        IDeviceRepository deviceRepository,
        IRegisterRepository registerRepository,
        IModbusLogRepository logRepository)
    {
        _deviceRepository = deviceRepository;
        _registerRepository = registerRepository;
        _logRepository = logRepository;
    }

    public async Task<RegisterOperationResult> ReadAsync(
        ReadRegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidSlaveAddress(request.SlaveAddress))
            return RegisterOperationResult.Rejected("Modbus slave address must be between 1 and 247.");

        if (!IsValidRegisterAddress(request.RegisterAddress))
            return RegisterOperationResult.Rejected("Register address cannot be negative.");

        var device = await _deviceRepository.GetBySlaveAddressAsync(
            request.SlaveAddress,
            cancellationToken);

        if (device is null)
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.ReadHoldingRegisters,
                request.RegisterAddress,
                ModbusOperationStatus.Failed,
                "Device not found.",
                cancellationToken: cancellationToken);

            return RegisterOperationResult.Failed("Device not found.");
        }

        if (!device.IsEnabled)
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.ReadHoldingRegisters,
                request.RegisterAddress,
                ModbusOperationStatus.Rejected,
                "Device is disabled.",
                device.Id,
                cancellationToken: cancellationToken);

            return RegisterOperationResult.Rejected("Device is disabled.");
        }

        var definition = await _registerRepository.GetDefinitionByAddressAsync(
            device.DeviceTypeId,
            request.RegisterAddress,
            cancellationToken);

        if (definition is null)
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.ReadHoldingRegisters,
                request.RegisterAddress,
                ModbusOperationStatus.Failed,
                "Register definition not found.",
                device.Id,
                cancellationToken: cancellationToken);

            return RegisterOperationResult.Failed("Register definition not found.");
        }

        var value = await _registerRepository.GetValueAsync(
            device.Id,
            definition.Id,
            cancellationToken);

        if (value is null)
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.ReadHoldingRegisters,
                request.RegisterAddress,
                ModbusOperationStatus.Failed,
                "Register value is not initialized.",
                device.Id,
                cancellationToken: cancellationToken);

            return RegisterOperationResult.Failed("Register value is not initialized.");
        }

        await AddLogAsync(
            request.SlaveAddress,
            ModbusFunctionCode.ReadHoldingRegisters,
            request.RegisterAddress,
            ModbusOperationStatus.Success,
            "Register read successfully.",
            device.Id,
            value.Value,
            cancellationToken);

        return RegisterOperationResult.Success(
            value.Value,
            "Register read successfully.");
    }

    public async Task<RegisterOperationResult> WriteAsync(
        WriteRegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidSlaveAddress(request.SlaveAddress))
            return RegisterOperationResult.Rejected("Modbus slave address must be between 1 and 247.");

        if (!IsValidRegisterAddress(request.RegisterAddress))
            return RegisterOperationResult.Rejected("Register address cannot be negative.");

        var device = await _deviceRepository.GetBySlaveAddressAsync(
            request.SlaveAddress,
            cancellationToken);

        if (device is null)
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.WriteSingleRegister,
                request.RegisterAddress,
                ModbusOperationStatus.Failed,
                "Device not found.",
                value: request.Value,
                cancellationToken: cancellationToken);

            return RegisterOperationResult.Failed("Device not found.");
        }

        if (!device.IsEnabled)
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.WriteSingleRegister,
                request.RegisterAddress,
                ModbusOperationStatus.Rejected,
                "Device is disabled.",
                device.Id,
                request.Value,
                cancellationToken);

            return RegisterOperationResult.Rejected("Device is disabled.");
        }

        var definition = await _registerRepository.GetDefinitionByAddressAsync(
            device.DeviceTypeId,
            request.RegisterAddress,
            cancellationToken);

        if (definition is null)
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.WriteSingleRegister,
                request.RegisterAddress,
                ModbusOperationStatus.Failed,
                "Register definition not found.",
                device.Id,
                request.Value,
                cancellationToken);

            return RegisterOperationResult.Failed("Register definition not found.");
        }

        if (!definition.CanWrite())
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.WriteSingleRegister,
                request.RegisterAddress,
                ModbusOperationStatus.Rejected,
                "Register is read-only.",
                device.Id,
                request.Value,
                cancellationToken);

            return RegisterOperationResult.Rejected("Register is read-only.");
        }

        if (!definition.IsValueInRange(request.Value))
        {
            await AddLogAsync(
                request.SlaveAddress,
                ModbusFunctionCode.WriteSingleRegister,
                request.RegisterAddress,
                ModbusOperationStatus.Rejected,
                "Register value is out of allowed range.",
                device.Id,
                request.Value,
                cancellationToken);

            return RegisterOperationResult.Rejected("Register value is out of allowed range.");
        }

        var registerValue = await _registerRepository.GetValueAsync(
            device.Id,
            definition.Id,
            cancellationToken);

        if (registerValue is null)
        {
            registerValue = new RegisterValue(
                device.Id,
                definition.Id,
                request.Value);
        }
        else
        {
            registerValue.UpdateValue(request.Value);
        }

        await _registerRepository.SaveValueAsync(registerValue, cancellationToken);

        await AddLogAsync(
            request.SlaveAddress,
            ModbusFunctionCode.WriteSingleRegister,
            request.RegisterAddress,
            ModbusOperationStatus.Success,
            "Register written successfully.",
            device.Id,
            request.Value,
            cancellationToken);

        return RegisterOperationResult.Success(
            request.Value,
            "Register written successfully.");
    }

    private async Task AddLogAsync(
        int slaveAddress,
        ModbusFunctionCode functionCode,
        int registerAddress,
        ModbusOperationStatus status,
        string message,
        Guid? slaveDeviceId = null,
        int? value = null,
        CancellationToken cancellationToken = default)
    {
        var logEntry = new ModbusLogEntry(
            slaveAddress,
            functionCode,
            registerAddress,
            status,
            message,
            slaveDeviceId,
            value);

        await _logRepository.AddAsync(logEntry, cancellationToken);
    }

    private static bool IsValidSlaveAddress(int slaveAddress)
    {
        return slaveAddress is >= 1 and <= 247;
    }

    private static bool IsValidRegisterAddress(int registerAddress)
    {
        return registerAddress >= 0;
    }
}