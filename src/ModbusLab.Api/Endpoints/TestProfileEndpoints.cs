using ModbusLab.Application.Testing;

namespace ModbusLab.Api.Endpoints;

public static class TestProfileEndpoints
{
    public static IEndpointRouteBuilder MapTestProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/test-profiles")
            .WithTags("Test profiles")
            .RequireAuthorization();

        group.MapGet("/", async (
            TestProfileService testProfileService,
            CancellationToken cancellationToken) =>
        {
            var profiles = await testProfileService.GetAllAsync(cancellationToken);

            return Results.Ok(profiles);
        })
        .WithSummary("Get all test profiles");

        group.MapGet("/{profileId:guid}", async (
            Guid profileId,
            TestProfileService testProfileService,
            CancellationToken cancellationToken) =>
        {
            var profile = await testProfileService.GetByIdAsync(profileId, cancellationToken);

            return profile is null
                ? Results.NotFound()
                : Results.Ok(profile);
        })
        .WithSummary("Get test profile by id");

        group.MapPost("/", async (
            CreateTestProfileRequest request,
            TestProfileService testProfileService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var profile = await testProfileService.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/test-profiles/{profile.Id}", profile);
            }
            catch (ArgumentException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Create test profile");

        group.MapPost("/{profileId:guid}/steps", async (
            Guid profileId,
            CreateTestStepRequest request,
            TestProfileService testProfileService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var profile = await testProfileService.AddStepAsync(
                    profileId,
                    request,
                    cancellationToken);

                return profile is null
                    ? Results.NotFound()
                    : Results.Ok(profile);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is ArgumentOutOfRangeException)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Add step to test profile");

        group.MapPost("/{profileId:guid}/run", async (
            Guid profileId,
            TestExecutionService testExecutionService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var run = await testExecutionService.RunProfileAsync(profileId, cancellationToken);

                return run is null
                    ? Results.NotFound()
                    : Results.Ok(run);
            }
            catch (InvalidOperationException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Run test profile");

        return app;
    }

    public static IEndpointRouteBuilder MapTestRunEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/test-runs")
            .WithTags("Test runs")
            .RequireAuthorization();

        group.MapGet("/", async (
            TestExecutionService testExecutionService,
            CancellationToken cancellationToken) =>
        {
            var runs = await testExecutionService.GetLatestRunsAsync(cancellationToken);

            return Results.Ok(runs);
        })
        .WithSummary("Get latest test runs");

        group.MapGet("/{runId:guid}", async (
            Guid runId,
            TestExecutionService testExecutionService,
            CancellationToken cancellationToken) =>
        {
            var run = await testExecutionService.GetRunAsync(runId, cancellationToken);

            return run is null
                ? Results.NotFound()
                : Results.Ok(run);
        })
        .WithSummary("Get test run by id");

        return app;
    }
}
