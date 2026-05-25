using Microsoft.EntityFrameworkCore;
using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Logs;

namespace ModbusLab.Infrastructure.Persistence.Repositories;

public sealed class EfModbusLogRepository : IModbusLogRepository
{
    private readonly ModbusLabDbContext _dbContext;

    public EfModbusLogRepository(ModbusLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ModbusLogEntry logEntry,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ModbusLogs.AddAsync(logEntry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ModbusLogEntry>> GetLatestAsync(
        int count = 100,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            count = 100;

        return await _dbContext.ModbusLogs
            .OrderByDescending(log => log.TimestampUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
