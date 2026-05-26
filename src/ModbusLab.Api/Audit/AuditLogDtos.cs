namespace ModbusLab.Api.Audit;

public sealed record AuditLogDto(
    Guid Id,
    DateTime TimestampUtc,
    Guid? UserId,
    string? UserName,
    string? UserRole,
    string Action,
    string? EntityType,
    string? EntityId,
    string? Details,
    string? IpAddress,
    bool IsSuccess);
