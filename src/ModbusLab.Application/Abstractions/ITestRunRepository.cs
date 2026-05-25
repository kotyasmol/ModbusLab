using ModbusLab.Domain.Testing;

namespace ModbusLab.Application.Abstractions;

public interface ITestRunRepository
{
    Task AddAsync(TestRun testRun, CancellationToken cancellationToken = default);

    Task AddStepResultAsync(TestStepResult result, CancellationToken cancellationToken = default);

    Task<TestRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestRun>> GetLatestAsync(
        int count = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestStepResult>> GetStepResultsAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
