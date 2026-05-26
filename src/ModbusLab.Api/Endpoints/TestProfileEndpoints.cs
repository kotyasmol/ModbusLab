using ModbusLab.Api.Audit;
using ModbusLab.Api.Auth;
using ModbusLab.Application.Testing;

namespace ModbusLab.Api.Endpoints;

public static class TestProfileEndpoints
{
    public static IEndpointRouteBuilder MapTestProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/test-profiles")
            .WithTags("Test profiles");

        group.MapGet("/", async (
            TestProfileService testProfileService,
            CancellationToken cancellationToken) =>
        {
            var profiles = await testProfileService.GetAllAsync(cancellationToken);

            return Results.Ok(profiles);
        })
        .RequireAuthorization(AuthPolicies.RequireViewer)
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
        .RequireAuthorization(AuthPolicies.RequireViewer)
        .WithSummary("Get test profile by id");

        group.MapPost("/", async (
            CreateTestProfileRequest request,
            TestProfileService testProfileService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var profile = await testProfileService.CreateAsync(request, cancellationToken);
                await auditLogService.LogAsync(
                    "testing.create_profile",
                    isSuccess: true,
                    entityType: "TestProfile",
                    entityId: profile.Id.ToString(),
                    details: $"Created profile '{profile.Name}'.",
                    cancellationToken: cancellationToken);

                return Results.Created($"/api/test-profiles/{profile.Id}", profile);
            }
            catch (ArgumentException exception)
            {
                await auditLogService.LogAsync(
                    "testing.create_profile",
                    isSuccess: false,
                    entityType: "TestProfile",
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .RequireAuthorization(AuthPolicies.RequireAdmin)
        .WithSummary("Create test profile");

        group.MapPost("/{profileId:guid}/steps", async (
            Guid profileId,
            CreateTestStepRequest request,
            TestProfileService testProfileService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var profile = await testProfileService.AddStepAsync(
                    profileId,
                    request,
                    cancellationToken);

                if (profile is null)
                {
                    await auditLogService.LogAsync(
                        "testing.add_step",
                        isSuccess: false,
                        entityType: "TestProfile",
                        entityId: profileId.ToString(),
                        details: "Profile not found.",
                        cancellationToken: cancellationToken);

                    return Results.NotFound();
                }

                await auditLogService.LogAsync(
                    "testing.add_step",
                    isSuccess: true,
                    entityType: "TestProfile",
                    entityId: profileId.ToString(),
                    details: $"Added step '{request.Name}'.",
                    cancellationToken: cancellationToken);

                return Results.Ok(profile);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is ArgumentOutOfRangeException)
            {
                await auditLogService.LogAsync(
                    "testing.add_step",
                    isSuccess: false,
                    entityType: "TestProfile",
                    entityId: profileId.ToString(),
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .RequireAuthorization(AuthPolicies.RequireAdmin)
        .WithSummary("Add step to test profile");

        group.MapPost("/{profileId:guid}/run", async (
            Guid profileId,
            TestExecutionService testExecutionService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var run = await testExecutionService.RunProfileAsync(profileId, cancellationToken);

                if (run is null)
                {
                    await auditLogService.LogAsync(
                        "testing.run_profile",
                        isSuccess: false,
                        entityType: "TestProfile",
                        entityId: profileId.ToString(),
                        details: "Profile not found.",
                        cancellationToken: cancellationToken);

                    return Results.NotFound();
                }

                await auditLogService.LogAsync(
                    "testing.run_profile",
                    isSuccess: true,
                    entityType: "TestRun",
                    entityId: run.Id.ToString(),
                    details: $"Started profile '{run.ProfileName}'.",
                    cancellationToken: cancellationToken);

                return Results.Ok(run);
            }
            catch (InvalidOperationException exception)
            {
                await auditLogService.LogAsync(
                    "testing.run_profile",
                    isSuccess: false,
                    entityType: "TestProfile",
                    entityId: profileId.ToString(),
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .RequireAuthorization(AuthPolicies.RequireEngineer)
        .WithSummary("Run test profile");

        return app;
    }

    public static IEndpointRouteBuilder MapTestRunEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/test-runs")
            .WithTags("Test runs");

        group.MapGet("/", async (
            TestExecutionService testExecutionService,
            CancellationToken cancellationToken) =>
        {
            var runs = await testExecutionService.GetLatestRunsAsync(cancellationToken);

            return Results.Ok(runs);
        })
        .RequireAuthorization(AuthPolicies.RequireViewer)
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
        .RequireAuthorization(AuthPolicies.RequireViewer)
        .WithSummary("Get test run by id");

        return app;
    }
}
