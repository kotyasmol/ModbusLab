namespace ModbusLab.Domain.Testing;

public sealed class TestStepResult
{
    public Guid Id { get; private set; }

    public Guid TestRunId { get; private set; }

    public Guid? TestStepId { get; private set; }

    public int OrderIndex { get; private set; }

    public string StepName { get; private set; }

    public TestStepType StepType { get; private set; }

    public TestStepResultStatus Status { get; private set; }

    public string Message { get; private set; }

    public int? ExpectedValue { get; private set; }

    public int? ActualValue { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime FinishedAtUtc { get; private set; }

    private TestStepResult()
    {
        StepName = string.Empty;
        Message = string.Empty;
    }

    private TestStepResult(
        Guid testRunId,
        Guid? testStepId,
        int orderIndex,
        string stepName,
        TestStepType stepType,
        TestStepResultStatus status,
        string message,
        DateTime startedAtUtc,
        int? expectedValue = null,
        int? actualValue = null)
    {
        if (testRunId == Guid.Empty)
            throw new ArgumentException("Test run id cannot be empty.", nameof(testRunId));

        if (orderIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(orderIndex), "Order index must be positive.");

        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("Step name cannot be empty.", nameof(stepName));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty.", nameof(message));

        Id = Guid.NewGuid();
        TestRunId = testRunId;
        TestStepId = testStepId;
        OrderIndex = orderIndex;
        StepName = stepName.Trim();
        StepType = stepType;
        Status = status;
        Message = message.Trim();
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
        StartedAtUtc = startedAtUtc;
        FinishedAtUtc = DateTime.UtcNow;
    }

    public static TestStepResult Passed(
        Guid testRunId,
        TestStep step,
        DateTime startedAtUtc,
        string message,
        int? expectedValue = null,
        int? actualValue = null)
    {
        return new TestStepResult(
            testRunId,
            step.Id,
            step.OrderIndex,
            step.Name,
            step.Type,
            TestStepResultStatus.Passed,
            message,
            startedAtUtc,
            expectedValue,
            actualValue);
    }

    public static TestStepResult Failed(
        Guid testRunId,
        TestStep step,
        DateTime startedAtUtc,
        string message,
        int? expectedValue = null,
        int? actualValue = null)
    {
        return new TestStepResult(
            testRunId,
            step.Id,
            step.OrderIndex,
            step.Name,
            step.Type,
            TestStepResultStatus.Failed,
            message,
            startedAtUtc,
            expectedValue,
            actualValue);
    }
}
