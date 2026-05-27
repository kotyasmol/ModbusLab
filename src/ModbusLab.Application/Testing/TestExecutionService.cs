using ModbusLab.Application.Abstractions;
using ModbusLab.Application.Modbus;
using ModbusLab.Domain.Testing;

namespace ModbusLab.Application.Testing;

public sealed class TestExecutionService
{
    private readonly ITestProfileRepository _testProfileRepository;
    private readonly ITestRunRepository _testRunRepository;
    private readonly ModbusRegisterService _modbusRegisterService;

    public TestExecutionService(
        ITestProfileRepository testProfileRepository,
        ITestRunRepository testRunRepository,
        ModbusRegisterService modbusRegisterService)
    {
        _testProfileRepository = testProfileRepository;
        _testRunRepository = testRunRepository;
        _modbusRegisterService = modbusRegisterService;
    }

    public async Task<IReadOnlyList<TestRunDto>> GetLatestRunsAsync(
        CancellationToken cancellationToken = default)
    {
        var runs = await _testRunRepository.GetLatestAsync(20, cancellationToken);
        var result = new List<TestRunDto>();

        foreach (var run in runs)
        {
            var steps = await _testRunRepository.GetStepResultsAsync(run.Id, cancellationToken);
            result.Add(ToDto(run, steps));
        }

        return result;
    }

    public async Task<TestRunDto?> GetRunAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default)
    {
        var run = await _testRunRepository.GetByIdAsync(testRunId, cancellationToken);

        if (run is null)
            return null;

        var steps = await _testRunRepository.GetStepResultsAsync(run.Id, cancellationToken);

        return ToDto(run, steps);
    }

    public async Task<TestRunDto?> RunProfileAsync(
        Guid testProfileId,
        CancellationToken cancellationToken = default)
    {
        var queuedRun = await QueueProfileRunAsync(testProfileId, cancellationToken);

        if (queuedRun is null)
            return null;

        return await ExecuteQueuedRunAsync(
            queuedRun.Id,
            publishProgressAsync: null,
            cancellationToken);
    }

    public async Task<TestRunDto?> QueueProfileRunAsync(
        Guid testProfileId,
        CancellationToken cancellationToken = default)
    {
        var profile = await _testProfileRepository.GetByIdAsync(testProfileId, cancellationToken);

        if (profile is null)
            return null;

        var steps = await _testProfileRepository.GetStepsAsync(testProfileId, cancellationToken);

        if (!profile.IsEnabled)
            throw new InvalidOperationException("Test profile is disabled.");

        if (steps.Count == 0)
            throw new InvalidOperationException("Test profile has no steps.");

        var testRun = new TestRun(profile.Id, profile.Name);

        await _testRunRepository.AddAsync(testRun, cancellationToken);
        await _testRunRepository.SaveChangesAsync(cancellationToken);

        return ToDto(testRun, Array.Empty<TestStepResult>());
    }

    public async Task<TestRunDto?> ExecuteQueuedRunAsync(
        Guid testRunId,
        Func<TestRunProgressEvent, CancellationToken, Task>? publishProgressAsync = null,
        CancellationToken cancellationToken = default)
    {
        var testRun = await _testRunRepository.GetByIdAsync(testRunId, cancellationToken);

        if (testRun is null)
            return null;

        var steps = await _testProfileRepository.GetStepsAsync(testRun.TestProfileId, cancellationToken);

        if (steps.Count == 0)
            throw new InvalidOperationException("Test profile has no steps.");

        testRun.MarkRunning();
        await _testRunRepository.SaveChangesAsync(cancellationToken);

        await PublishProgressAsync(
            publishProgressAsync,
            testRun,
            completedSteps: 0,
            totalSteps: steps.Count,
            "Test run started.",
            cancellationToken);

        var completedSteps = 0;

        foreach (var step in steps.OrderBy(step => step.OrderIndex))
        {
            var result = await ExecuteStepAsync(testRun.Id, step, cancellationToken);

            await _testRunRepository.AddStepResultAsync(result, cancellationToken);
            completedSteps++;

            await _testRunRepository.SaveChangesAsync(cancellationToken);

            await PublishProgressAsync(
                publishProgressAsync,
                testRun,
                completedSteps,
                steps.Count,
                $"Step {step.OrderIndex} {result.Status}: {step.Name}",
                cancellationToken);

            if (result.Status == TestStepResultStatus.Failed)
            {
                testRun.CompleteAsFailed($"Step {step.OrderIndex} failed: {step.Name}");
                await _testRunRepository.SaveChangesAsync(cancellationToken);

                await PublishProgressAsync(
                    publishProgressAsync,
                    testRun,
                    completedSteps,
                    steps.Count,
                    testRun.Summary ?? "Test run failed.",
                    cancellationToken);

                var failedSteps = await _testRunRepository.GetStepResultsAsync(testRun.Id, cancellationToken);
                return ToDto(testRun, failedSteps);
            }
        }

        testRun.CompleteAsPassed("All test steps completed successfully.");
        await _testRunRepository.SaveChangesAsync(cancellationToken);

        await PublishProgressAsync(
            publishProgressAsync,
            testRun,
            completedSteps,
            steps.Count,
            testRun.Summary ?? "Test run passed.",
            cancellationToken);

        var stepResults = await _testRunRepository.GetStepResultsAsync(testRun.Id, cancellationToken);

        return ToDto(testRun, stepResults);
    }

    private async Task<TestStepResult> ExecuteStepAsync(
        Guid testRunId,
        TestStep step,
        CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTime.UtcNow;

        try
        {
            return step.Type switch
            {
                TestStepType.WriteRegister => await ExecuteWriteStepAsync(
                    testRunId,
                    step,
                    startedAtUtc,
                    cancellationToken),

                TestStepType.Delay => await ExecuteDelayStepAsync(
                    testRunId,
                    step,
                    startedAtUtc,
                    cancellationToken),

                TestStepType.CheckRegisterRange => await ExecuteCheckRangeStepAsync(
                    testRunId,
                    step,
                    startedAtUtc,
                    cancellationToken),

                _ => TestStepResult.Failed(
                    testRunId,
                    step,
                    startedAtUtc,
                    "Unsupported test step type.")
            };
        }
        catch (Exception exception)
        {
            return TestStepResult.Failed(
                testRunId,
                step,
                startedAtUtc,
                exception.Message);
        }
    }

    private async Task<TestStepResult> ExecuteWriteStepAsync(
        Guid testRunId,
        TestStep step,
        DateTime startedAtUtc,
        CancellationToken cancellationToken)
    {
        var request = new WriteRegisterRequest(
            step.SlaveAddress!.Value,
            step.RegisterAddress!.Value,
            step.Value!.Value);

        var operationResult = await _modbusRegisterService.WriteAsync(request, cancellationToken);

        if (!operationResult.IsSuccess)
        {
            return TestStepResult.Failed(
                testRunId,
                step,
                startedAtUtc,
                operationResult.Message,
                step.Value,
                operationResult.Value);
        }

        return TestStepResult.Passed(
            testRunId,
            step,
            startedAtUtc,
            operationResult.Message,
            step.Value,
            operationResult.Value);
    }

    private static async Task<TestStepResult> ExecuteDelayStepAsync(
        Guid testRunId,
        TestStep step,
        DateTime startedAtUtc,
        CancellationToken cancellationToken)
    {
        await Task.Delay(step.DelayMs!.Value, cancellationToken);

        return TestStepResult.Passed(
            testRunId,
            step,
            startedAtUtc,
            $"Delay completed: {step.DelayMs} ms.");
    }

    private async Task<TestStepResult> ExecuteCheckRangeStepAsync(
        Guid testRunId,
        TestStep step,
        DateTime startedAtUtc,
        CancellationToken cancellationToken)
    {
        var request = new ReadRegisterRequest(
            step.SlaveAddress!.Value,
            step.RegisterAddress!.Value);

        var operationResult = await _modbusRegisterService.ReadAsync(request, cancellationToken);

        if (!operationResult.IsSuccess)
        {
            return TestStepResult.Failed(
                testRunId,
                step,
                startedAtUtc,
                operationResult.Message,
                expectedValue: null,
                actualValue: operationResult.Value);
        }

        var actualValue = operationResult.Value;

        if (!actualValue.HasValue)
        {
            return TestStepResult.Failed(
                testRunId,
                step,
                startedAtUtc,
                "Register value is empty.");
        }

        if (actualValue.Value < step.MinValue!.Value || actualValue.Value > step.MaxValue!.Value)
        {
            return TestStepResult.Failed(
                testRunId,
                step,
                startedAtUtc,
                $"Value {actualValue.Value} is outside allowed range {step.MinValue}-{step.MaxValue}.",
                expectedValue: null,
                actualValue: actualValue.Value);
        }

        return TestStepResult.Passed(
            testRunId,
            step,
            startedAtUtc,
            $"Value {actualValue.Value} is inside allowed range {step.MinValue}-{step.MaxValue}.",
            expectedValue: null,
            actualValue: actualValue.Value);
    }

    private static TestRunDto ToDto(TestRun run, IReadOnlyList<TestStepResult> stepResults)
    {
        return new TestRunDto(
            run.Id,
            run.TestProfileId,
            run.ProfileName,
            run.Status.ToString(),
            run.StartedAtUtc,
            run.FinishedAtUtc,
            run.Summary,
            stepResults
                .OrderBy(step => step.OrderIndex)
                .Select(ToDto)
                .ToList());
    }

    private static Task PublishProgressAsync(
        Func<TestRunProgressEvent, CancellationToken, Task>? publishProgressAsync,
        TestRun run,
        int completedSteps,
        int totalSteps,
        string message,
        CancellationToken cancellationToken)
    {
        if (publishProgressAsync is null)
            return Task.CompletedTask;

        return publishProgressAsync(
            new TestRunProgressEvent(
                run.Id,
                run.TestProfileId,
                run.ProfileName,
                run.Status.ToString(),
                completedSteps,
                totalSteps,
                message,
                DateTime.UtcNow),
            cancellationToken);
    }

    private static TestStepResultDto ToDto(TestStepResult result)
    {
        return new TestStepResultDto(
            result.Id,
            result.OrderIndex,
            result.StepName,
            result.StepType.ToString(),
            result.Status.ToString(),
            result.Message,
            result.ExpectedValue,
            result.ActualValue,
            result.StartedAtUtc,
            result.FinishedAtUtc);
    }
}
