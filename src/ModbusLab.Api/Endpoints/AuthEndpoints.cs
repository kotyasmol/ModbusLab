using System.Security.Claims;
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
            CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await authService.RegisterAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .AllowAnonymous()
        .WithSummary("Register a local user");

        group.MapPost("/login", async (
            LoginRequest request,
            AuthService authService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var response = await authService.LoginAsync(request, cancellationToken);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .AllowAnonymous()
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
