namespace ModbusLab.Domain.Logs;

public sealed class AuditLogEntry
{
    public Guid Id { get; private set; }

    public DateTime TimestampUtc { get; private set; }

    public Guid? UserId { get; private set; }

    public string? UserName { get; private set; }

    public string? UserRole { get; private set; }

    public string Action { get; private set; }

    public string? EntityType { get; private set; }

    public string? EntityId { get; private set; }

    public string? Details { get; private set; }

    public string? IpAddress { get; private set; }

    public bool IsSuccess { get; private set; }

    private AuditLogEntry()
    {
        Action = string.Empty;
    }

    public AuditLogEntry(
        Guid? userId,
        string? userName,
        string? userRole,
        string action,
        string? entityType,
        string? entityId,
        string? details,
        string? ipAddress,
        bool isSuccess)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Audit action cannot be empty.", nameof(action));

        Id = Guid.NewGuid();
        TimestampUtc = DateTime.UtcNow;
        UserId = userId;
        UserName = TrimToNull(userName);
        UserRole = TrimToNull(userRole);
        Action = action.Trim();
        EntityType = TrimToNull(entityType);
        EntityId = TrimToNull(entityId);
        Details = TrimToNull(details);
        IpAddress = TrimToNull(ipAddress);
        IsSuccess = isSuccess;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
