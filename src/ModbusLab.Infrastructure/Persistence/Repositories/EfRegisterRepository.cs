using Microsoft.EntityFrameworkCore;
using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Registers;

namespace ModbusLab.Infrastructure.Persistence.Repositories;

public sealed class EfRegisterRepository : IRegisterRepository
{
    private readonly ModbusLabDbContext _dbContext;

    public EfRegisterRepository(ModbusLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RegisterDefinition>> GetDefinitionsByDeviceTypeIdAsync(
        Guid deviceTypeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RegisterDefinitions
            .Where(definition => definition.DeviceTypeId == deviceTypeId)
            .OrderBy(definition => definition.Address)
            .ToListAsync(cancellationToken);
    }

    public async Task<RegisterDefinition?> GetDefinitionByAddressAsync(
        Guid deviceTypeId,
        int address,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RegisterDefinitions
            .FirstOrDefaultAsync(
                definition =>
                    definition.DeviceTypeId == deviceTypeId &&
                    definition.Address == address,
                cancellationToken);
    }

    public async Task<RegisterValue?> GetValueAsync(
        Guid slaveDeviceId,
        Guid registerDefinitionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.RegisterValues
            .FirstOrDefaultAsync(
                value =>
                    value.SlaveDeviceId == slaveDeviceId &&
                    value.RegisterDefinitionId == registerDefinitionId,
                cancellationToken);
    }

    public async Task SaveValueAsync(
        RegisterValue registerValue,
        CancellationToken cancellationToken = default)
    {
        var existingValue = await _dbContext.RegisterValues
            .FirstOrDefaultAsync(
                value =>
                    value.SlaveDeviceId == registerValue.SlaveDeviceId &&
                    value.RegisterDefinitionId == registerValue.RegisterDefinitionId,
                cancellationToken);

        if (existingValue is null)
        {
            await _dbContext.RegisterValues.AddAsync(registerValue, cancellationToken);
        }
        else
        {
            existingValue.UpdateValue(registerValue.Value);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
