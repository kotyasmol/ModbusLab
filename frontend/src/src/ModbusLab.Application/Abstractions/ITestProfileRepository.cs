using ModbusLab.Domain.Testing;

namespace ModbusLab.Application.Abstractions;

public interface ITestProfileRepository
{
    Task<IReadOnlyList<TestProfile>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TestProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestStep>> GetStepsAsync(
        Guid testProfileId,
        CancellationToken cancellationToken = default);

    Task AddAsync(TestProfile profile, CancellationToken cancellationToken = default);

    Task AddStepAsync(TestStep step, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
