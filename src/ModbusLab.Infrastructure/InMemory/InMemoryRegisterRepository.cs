using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Registers;

namespace ModbusLab.Infrastructure.InMemory;

public sealed class InMemoryRegisterRepository : IRegisterRepository
{
    private readonly InMemoryStore _store;

    public InMemoryRegisterRepository(InMemoryStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<RegisterDefinition>> GetDefinitionsByDeviceTypeIdAsync(
        Guid deviceTypeId,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            var definitions = _store.RegisterDefinitions
                .Where(definition => definition.DeviceTypeId == deviceTypeId)
                .OrderBy(definition => definition.Address)
                .ToList();

            return Task.FromResult<IReadOnlyList<RegisterDefinition>>(definitions);
        }
    }

    public Task<RegisterDefinition?> GetDefinitionByAddressAsync(
        Guid deviceTypeId,
        int address,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            var definition = _store.RegisterDefinitions.FirstOrDefault(
                definition =>
                    definition.DeviceTypeId == deviceTypeId &&
                    definition.Address == address);

            return Task.FromResult(definition);
        }
    }

    public Task<RegisterValue?> GetValueAsync(
        Guid slaveDeviceId,
        Guid registerDefinitionId,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            var value = _store.RegisterValues.FirstOrDefault(
                value =>
                    value.SlaveDeviceId == slaveDeviceId &&
                    value.RegisterDefinitionId == registerDefinitionId);

            return Task.FromResult(value);
        }
    }

    public Task SaveValueAsync(
        RegisterValue registerValue,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            var existingValue = _store.RegisterValues.FirstOrDefault(
                value =>
                    value.SlaveDeviceId == registerValue.SlaveDeviceId &&
                    value.RegisterDefinitionId == registerValue.RegisterDefinitionId);

            if (existingValue is null)
            {
                _store.RegisterValues.Add(registerValue);
            }
            else
            {
                existingValue.UpdateValue(registerValue.Value);
            }
        }

        return Task.CompletedTask;
    }
}