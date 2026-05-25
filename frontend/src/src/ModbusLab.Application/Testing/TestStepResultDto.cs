namespace ModbusLab.Application.Testing;

public sealed record TestStepResultDto(
    Guid Id,
    int OrderIndex,
    string StepName,
    string StepType,
    string Status,
    string Message,
    int? ExpectedValue,
    int? ActualValue,
    DateTime StartedAtUtc,
    DateTime FinishedAtUtc);
