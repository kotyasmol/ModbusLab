using ModbusLab.Domain.Registers;

namespace ModbusLab.Application.Abstractions;

public interface IRegisterRepository
{
    Task<IReadOnlyList<RegisterDefinition>> GetDefinitionsByDeviceTypeIdAsync(
        Guid deviceTypeId,
        CancellationToken cancellationToken = default);

    Task<RegisterDefinition?> GetDefinitionByAddressAsync(
        Guid deviceTypeId,
        int address,
        CancellationToken cancellationToken = default);

    Task<RegisterValue?> GetValueAsync(
        Guid slaveDeviceId,
        Guid registerDefinitionId,
        CancellationToken cancellationToken = default);

    Task SaveValueAsync(RegisterValue registerValue, CancellationToken cancellationToken = default);
}