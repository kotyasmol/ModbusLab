using Microsoft.EntityFrameworkCore;
using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Devices;

namespace ModbusLab.Infrastructure.Persistence.Repositories;

public sealed class EfDeviceRepository : IDeviceRepository
{
    private readonly ModbusLabDbContext _dbContext;

    public EfDeviceRepository(ModbusLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SlaveDevice>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SlaveDevices
            .OrderBy(device => device.SlaveAddress)
            .ToListAsync(cancellationToken);
    }

    public async Task<SlaveDevice?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SlaveDevices
            .FirstOrDefaultAsync(device => device.Id == id, cancellationToken);
    }

    public async Task<SlaveDevice?> GetBySlaveAddressAsync(
        int slaveAddress,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SlaveDevices
            .FirstOrDefaultAsync(device => device.SlaveAddress == slaveAddress, cancellationToken);
    }

    public async Task AddAsync(
        SlaveDevice device,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SlaveDevices.AddAsync(device, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
