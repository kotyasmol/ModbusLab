namespace ModbusLab.Application.Testing;

public sealed record TestRunProgressEvent(
    Guid TestRunId,
    Guid TestProfileId,
    string ProfileName,
    string Status,
    int CompletedSteps,
    int TotalSteps,
    string Message,
    DateTime TimestampUtc);
