namespace ModbusLab.Domain.Testing;

public sealed class TestRun
{
    public Guid Id { get; private set; }

    public Guid TestProfileId { get; private set; }

    public string ProfileName { get; private set; }

    public TestRunStatus Status { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? FinishedAtUtc { get; private set; }

    public string? Summary { get; private set; }

    private TestRun()
    {
        ProfileName = string.Empty;
    }

    public TestRun(Guid testProfileId, string profileName)
    {
        if (testProfileId == Guid.Empty)
            throw new ArgumentException("Test profile id cannot be empty.", nameof(testProfileId));

        if (string.IsNullOrWhiteSpace(profileName))
            throw new ArgumentException("Test profile name cannot be empty.", nameof(profileName));

        Id = Guid.NewGuid();
        TestProfileId = testProfileId;
        ProfileName = profileName.Trim();
        Status = TestRunStatus.Queued;
        StartedAtUtc = DateTime.UtcNow;
    }

    public void MarkRunning()
    {
        Status = TestRunStatus.Running;
        Summary = "Test run is executing.";
    }

    public void CompleteAsPassed(string summary)
    {
        Status = TestRunStatus.Passed;
        Summary = summary;
        FinishedAtUtc = DateTime.UtcNow;
    }

    public void CompleteAsFailed(string summary)
    {
        Status = TestRunStatus.Failed;
        Summary = summary;
        FinishedAtUtc = DateTime.UtcNow;
    }
}
