using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ModbusLab.Domain.Logs;
using ModbusLab.Infrastructure.Persistence;

namespace ModbusLab.Api.Audit;

public sealed class AuditLogService
{
    private const int MaxDetailsLength = 1024;

    private readonly ModbusLabDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogService(
        ModbusLabDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(
        string action,
        bool isSuccess,
        string? entityType = null,
        string? entityId = null,
        string? details = null,
        string? userNameOverride = null,
        CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var principal = httpContext?.User;
        var userIdValue = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = userNameOverride ?? principal?.FindFirstValue(ClaimTypes.Name);
        var userRole = principal?.FindFirstValue(ClaimTypes.Role);

        var entry = new AuditLogEntry(
            Guid.TryParse(userIdValue, out var userId) ? userId : null,
            userName,
            userRole,
            action,
            entityType,
            entityId,
            TrimDetails(details),
            httpContext?.Connection.RemoteIpAddress?.ToString(),
            isSuccess);

        await _dbContext.AuditLogs.AddAsync(entry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> GetLatestAsync(
        int count,
        CancellationToken cancellationToken)
    {
        count = Math.Clamp(count, 1, 500);

        return await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.TimestampUtc)
            .Take(count)
            .Select(log => new AuditLogDto(
                log.Id,
                log.TimestampUtc,
                log.UserId,
                log.UserName,
                log.UserRole,
                log.Action,
                log.EntityType,
                log.EntityId,
                log.Details,
                log.IpAddress,
                log.IsSuccess))
            .ToListAsync(cancellationToken);
    }

    private static string? TrimDetails(string? details)
    {
        if (string.IsNullOrWhiteSpace(details))
            return null;

        var trimmed = details.Trim();
        return trimmed.Length <= MaxDetailsLength
            ? trimmed
            : trimmed[..MaxDetailsLength];
    }
}
