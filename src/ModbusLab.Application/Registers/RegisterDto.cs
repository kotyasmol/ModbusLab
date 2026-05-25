namespace ModbusLab.Application.Registers;

public sealed record RegisterDto(
    Guid DefinitionId,
    int Address,
    string Name,
    string AccessMode,
    string? Unit,
    int? MinValue,
    int? MaxValue,
    int? CurrentValue,
    DateTime? UpdatedAtUtc);
