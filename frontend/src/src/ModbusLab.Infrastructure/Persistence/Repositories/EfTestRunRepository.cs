using Microsoft.EntityFrameworkCore;
using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Testing;

namespace ModbusLab.Infrastructure.Persistence.Repositories;

public sealed class EfTestRunRepository : ITestRunRepository
{
    private readonly ModbusLabDbContext _dbContext;

    public EfTestRunRepository(ModbusLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        TestRun testRun,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.TestRuns.AddAsync(testRun, cancellationToken);
    }

    public async Task AddStepResultAsync(
        TestStepResult result,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.TestStepResults.AddAsync(result, cancellationToken);
    }

    public async Task<TestRun?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TestRuns
            .FirstOrDefaultAsync(run => run.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TestRun>> GetLatestAsync(
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            count = 20;

        return await _dbContext.TestRuns
            .OrderByDescending(run => run.StartedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestStepResult>> GetStepResultsAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TestStepResults
            .Where(result => result.TestRunId == testRunId)
            .OrderBy(result => result.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
