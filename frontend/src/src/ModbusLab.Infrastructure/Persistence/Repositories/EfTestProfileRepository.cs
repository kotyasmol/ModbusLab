using Microsoft.EntityFrameworkCore;
using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Testing;

namespace ModbusLab.Infrastructure.Persistence.Repositories;

public sealed class EfTestProfileRepository : ITestProfileRepository
{
    private readonly ModbusLabDbContext _dbContext;

    public EfTestProfileRepository(ModbusLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TestProfile>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TestProfiles
            .OrderBy(profile => profile.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TestProfile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TestProfiles
            .FirstOrDefaultAsync(profile => profile.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TestStep>> GetStepsAsync(
        Guid testProfileId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TestSteps
            .Where(step => step.TestProfileId == testProfileId)
            .OrderBy(step => step.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TestProfile profile,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.TestProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStepAsync(
        TestStep step,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.TestSteps.AddAsync(step, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
