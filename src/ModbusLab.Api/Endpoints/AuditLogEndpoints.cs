using ModbusLab.Api.Audit;
using ModbusLab.Api.Auth;

namespace ModbusLab.Api.Endpoints;

public static class AuditLogEndpoints
{
    public static IEndpointRouteBuilder MapAuditLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/audit-logs")
            .WithTags("Audit logs")
            .RequireAuthorization(AuthPolicies.RequireAdmin);

        group.MapGet("/", async (
            int? count,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            var logs = await auditLogService.GetLatestAsync(count ?? 100, cancellationToken);
            return Results.Ok(logs);
        })
        .WithSummary("Get latest user audit log entries");

        return app;
    }
}
