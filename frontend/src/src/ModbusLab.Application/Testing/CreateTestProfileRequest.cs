namespace ModbusLab.Application.Testing;

public sealed record CreateTestProfileRequest(
    string Name,
    string? Description);
