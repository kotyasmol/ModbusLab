namespace ModbusLab.Application.Testing;

public sealed record TestProfileDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsEnabled,
    DateTime CreatedAtUtc,
    IReadOnlyList<TestStepDto> Steps);
