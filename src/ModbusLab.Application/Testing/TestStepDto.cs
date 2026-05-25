namespace ModbusLab.Application.Testing;

public sealed record TestStepDto(
    Guid Id,
    int OrderIndex,
    string Name,
    string Type,
    int? SlaveAddress,
    int? RegisterAddress,
    int? Value,
    int? MinValue,
    int? MaxValue,
    int? DelayMs);
