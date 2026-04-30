using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Logs;

namespace ModbusLab.Infrastructure.InMemory;

public sealed class InMemoryModbusLogRepository : IModbusLogRepository
{
    private readonly InMemoryStore _store;

    public InMemoryModbusLogRepository(InMemoryStore store)
    {
        _store = store;
    }

    public Task AddAsync(
        ModbusLogEntry logEntry,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            _store.Logs.Add(logEntry);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ModbusLogEntry>> GetLatestAsync(
        int count = 100,
        CancellationToken cancellationToken = default)
    {
        lock (_store.SyncRoot)
        {
            var logs = _store.Logs
                .OrderByDescending(log => log.TimestampUtc)
                .Take(count)
                .ToList();

            return Task.FromResult<IReadOnlyList<ModbusLogEntry>>(logs);
        }
    }
}