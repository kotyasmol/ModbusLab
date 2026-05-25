namespace ModbusLab.Application.Testing;

public sealed record TestRunDto(
    Guid Id,
    Guid TestProfileId,
    string ProfileName,
    string Status,
    DateTime StartedAtUtc,
    DateTime? FinishedAtUtc,
    string? Summary,
    IReadOnlyList<TestStepResultDto> Steps);
