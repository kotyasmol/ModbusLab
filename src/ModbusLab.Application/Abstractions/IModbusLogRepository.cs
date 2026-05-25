using ModbusLab.Domain.Logs;

namespace ModbusLab.Application.Abstractions;

public interface IModbusLogRepository
{
    Task AddAsync(ModbusLogEntry logEntry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModbusLogEntry>> GetLatestAsync(
        int count = 100,
        CancellationToken cancellationToken = default);
}
