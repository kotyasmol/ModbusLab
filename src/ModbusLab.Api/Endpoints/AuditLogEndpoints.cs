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
            string? action,
            string? userName,
            bool? isSuccess,
            DateTime? fromUtc,
            DateTime? toUtc,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            var logs = await auditLogService.GetLatestAsync(
                count ?? 100,
                action,
                userName,
                isSuccess,
                fromUtc,
                toUtc,
                cancellationToken);

            return Results.Ok(logs);
        })
        .WithSummary("Get filtered user audit log entries");

        return app;
    }
}
