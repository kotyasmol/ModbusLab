using ModbusLab.Infrastructure.Persistence;

namespace ModbusLab.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/health")
            .WithTags("Health")
            .AllowAnonymous();

        group.MapGet("/", () => Results.Ok(new
        {
            status = "Healthy",
            service = "ModbusLab.Api",
            timestampUtc = DateTime.UtcNow
        }))
        .WithSummary("Get API health status");

        group.MapGet("/db", async (
            ModbusLabDbContext dbContext,
            CancellationToken cancellationToken) =>
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? Results.Ok(new
                {
                    status = "Healthy",
                    service = "PostgreSQL",
                    timestampUtc = DateTime.UtcNow
                })
                : Results.Problem(
                    statusCode: StatusCodes.Status503ServiceUnavailable,
                    title: "Database is unavailable");
        })
        .WithSummary("Get database health status");

        return app;
    }
}
