namespace ModbusLab.Application.Testing;

public sealed record CreateTestStepRequest(
    string Type,
    string Name,
    int? SlaveAddress,
    int? RegisterAddress,
    int? Value,
    int? MinValue,
    int? MaxValue,
    int? DelayMs);
