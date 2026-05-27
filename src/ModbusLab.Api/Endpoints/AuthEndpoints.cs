using System.Security.Claims;
using ModbusLab.Api.Audit;
using ModbusLab.Api.Auth;

namespace ModbusLab.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            AuthService authService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request, cancellationToken);
                await auditLogService.LogAsync(
                    "auth.register",
                    isSuccess: true,
                    entityType: "AppUser",
                    entityId: response.User.Id.ToString(),
                    details: $"Registered user '{response.User.UserName}' with role '{response.User.Role}'.",
                    userNameOverride: response.User.UserName,
                    cancellationToken: cancellationToken);

                return Results.Ok(response);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                await auditLogService.LogAsync(
                    "auth.register",
                    isSuccess: false,
                    entityType: "AppUser",
                    details: exception.Message,
                    userNameOverride: request.UserName,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .AllowAnonymous()
        .RequireRateLimiting("auth")
        .WithSummary("Register a local user");

        group.MapPost("/login", async (
            LoginRequest request,
            AuthService authService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await authService.LoginAsync(request, cancellationToken);
                await auditLogService.LogAsync(
                    "auth.login",
                    isSuccess: true,
                    entityType: "AppUser",
                    entityId: response.User.Id.ToString(),
                    details: $"User '{response.User.UserName}' logged in.",
                    userNameOverride: response.User.UserName,
                    cancellationToken: cancellationToken);

                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                await auditLogService.LogAsync(
                    "auth.login",
                    isSuccess: false,
                    entityType: "AppUser",
                    details: "Invalid user name or password.",
                    userNameOverride: request.UserName,
                    cancellationToken: cancellationToken);

                return Results.Json(
                    new { message = "Invalid user name or password, or account is disabled." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }
        })
        .AllowAnonymous()
        .RequireRateLimiting("auth")
        .WithSummary("Login and receive JWT access token");

        group.MapGet("/me", async (
            ClaimsPrincipal user,
            AuthService authService,
            CancellationToken cancellationToken) =>
        {
            var currentUser = await authService.GetCurrentUserAsync(user, cancellationToken);
            return currentUser is null ? Results.Unauthorized() : Results.Ok(currentUser);
        })
        .RequireAuthorization()
        .WithSummary("Get current authenticated user");

        return app;
    }
}
