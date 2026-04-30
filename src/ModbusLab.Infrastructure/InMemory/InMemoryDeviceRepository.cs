using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Devices;

namespace ModbusLab.Infrastructure.InMemory;

public sealed class InMemoryDeviceRepository : IDeviceRepository
{
    private readonly InMemoryStore _store;

    public InMemoryDeviceRepository(InMemoryStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<SlaveDevice>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            return Task.FromResult<IReadOnlyList<SlaveDevice>>(
                _store.Devices.ToList());
        }
    }

    public Task<SlaveDevice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            var device = _store.Devices.FirstOrDefault(device => device.Id == id);
            return Task.FromResult(device);
        }
    }

    public Task<SlaveDevice?> GetBySlaveAddressAsync(
        int slaveAddress,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            var device = _store.Devices.FirstOrDefault(
                device => device.SlaveAddress == slaveAddress);

            return Task.FromResult(device);
        }
    }

    public Task AddAsync(
        SlaveDevice device,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            _store.Devices.Add(device);
        }

        return Task.CompletedTask;
    }
}