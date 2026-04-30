namespace ModbusLab.Api.Realtime;

public sealed record RegisterValueChangedEvent(
    Guid DeviceId,
    Guid RegisterDefinitionId,
    int RegisterAddress,
    int Value,
    DateTime UpdatedAtUtc);