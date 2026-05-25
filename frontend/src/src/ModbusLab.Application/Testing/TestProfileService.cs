using ModbusLab.Application.Abstractions;
using ModbusLab.Domain.Testing;

namespace ModbusLab.Application.Testing;

public sealed class TestProfileService
{
    private readonly ITestProfileRepository _testProfileRepository;

    public TestProfileService(ITestProfileRepository testProfileRepository)
    {
        _testProfileRepository = testProfileRepository;
    }

    public async Task<IReadOnlyList<TestProfileDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var profiles = await _testProfileRepository.GetAllAsync(cancellationToken);

        var result = new List<TestProfileDto>();

        foreach (var profile in profiles)
        {
            var steps = await _testProfileRepository.GetStepsAsync(profile.Id, cancellationToken);
            result.Add(ToDto(profile, steps));
        }

        return result;
    }

    public async Task<TestProfileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile = await _testProfileRepository.GetByIdAsync(id, cancellationToken);

        if (profile is null)
            return null;

        var steps = await _testProfileRepository.GetStepsAsync(profile.Id, cancellationToken);

        return ToDto(profile, steps);
    }

    public async Task<TestProfileDto> CreateAsync(
        CreateTestProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = new TestProfile(request.Name, request.Description);

        await _testProfileRepository.AddAsync(profile, cancellationToken);

        return ToDto(profile, Array.Empty<TestStep>());
    }

    public async Task<TestProfileDto?> AddStepAsync(
        Guid testProfileId,
        CreateTestStepRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await _testProfileRepository.GetByIdAsync(testProfileId, cancellationToken);

        if (profile is null)
            return null;

        var existingSteps = await _testProfileRepository.GetStepsAsync(testProfileId, cancellationToken);
        var orderIndex = existingSteps.Count + 1;

        var step = CreateStep(testProfileId, orderIndex, request);

        await _testProfileRepository.AddStepAsync(step, cancellationToken);

        var updatedSteps = await _testProfileRepository.GetStepsAsync(testProfileId, cancellationToken);

        return ToDto(profile, updatedSteps);
    }

    private static TestStep CreateStep(
        Guid testProfileId,
        int orderIndex,
        CreateTestStepRequest request)
    {
        var type = ParseStepType(request.Type);

        return type switch
        {
            TestStepType.WriteRegister => TestStep.CreateWriteRegister(
                testProfileId,
                orderIndex,
                request.Name,
                Require(request.SlaveAddress, nameof(request.SlaveAddress)),
                Require(request.RegisterAddress, nameof(request.RegisterAddress)),
                Require(request.Value, nameof(request.Value))),

            TestStepType.Delay => TestStep.CreateDelay(
                testProfileId,
                orderIndex,
                request.Name,
                Require(request.DelayMs, nameof(request.DelayMs))),

            TestStepType.CheckRegisterRange => TestStep.CreateCheckRegisterRange(
                testProfileId,
                orderIndex,
                request.Name,
                Require(request.SlaveAddress, nameof(request.SlaveAddress)),
                Require(request.RegisterAddress, nameof(request.RegisterAddress)),
                Require(request.MinValue, nameof(request.MinValue)),
                Require(request.MaxValue, nameof(request.MaxValue))),

            _ => throw new ArgumentOutOfRangeException(nameof(request.Type), "Unsupported test step type.")
        };
    }

    private static TestStepType ParseStepType(string type)
    {
        if (Enum.TryParse<TestStepType>(type, ignoreCase: true, out var parsed))
            return parsed;

        return type.Trim().ToLowerInvariant() switch
        {
            "write" => TestStepType.WriteRegister,
            "delay" => TestStepType.Delay,
            "check" => TestStepType.CheckRegisterRange,
            "checkrange" => TestStepType.CheckRegisterRange,
            _ => throw new ArgumentException("Unsupported test step type.", nameof(type))
        };
    }

    private static int Require(int? value, string propertyName)
    {
        return value ?? throw new ArgumentException($"Property '{propertyName}' is required.");
    }

    private static TestProfileDto ToDto(TestProfile profile, IReadOnlyList<TestStep> steps)
    {
        return new TestProfileDto(
            profile.Id,
            profile.Name,
            profile.Description,
            profile.IsEnabled,
            profile.CreatedAtUtc,
            steps
                .OrderBy(step => step.OrderIndex)
                .Select(ToDto)
                .ToList());
    }

    private static TestStepDto ToDto(TestStep step)
    {
        return new TestStepDto(
            step.Id,
            step.OrderIndex,
            step.Name,
            step.Type.ToString(),
            step.SlaveAddress,
            step.RegisterAddress,
            step.Value,
            step.MinValue,
            step.MaxValue,
            step.DelayMs);
    }
}
