using ModbusLab.Domain.Devices;

namespace ModbusLab.Application.Abstractions;

public interface IDeviceRepository
{
    Task<IReadOnlyList<SlaveDevice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SlaveDevice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SlaveDevice?> GetBySlaveAddressAsync(int slaveAddress, CancellationToken cancellationToken = default);

    Task AddAsync(SlaveDevice device, CancellationToken cancellationToken = default);
}
