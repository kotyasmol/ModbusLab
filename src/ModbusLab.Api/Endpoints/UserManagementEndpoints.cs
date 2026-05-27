using System.Security.Claims;
using ModbusLab.Api.Audit;
using ModbusLab.Api.Auth;

namespace ModbusLab.Api.Endpoints;

public static class UserManagementEndpoints
{
    public static IEndpointRouteBuilder MapUserManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/users")
            .RequireAuthorization(AuthPolicies.RequireAdmin)
            .WithTags("Users");

        group.MapGet("/", async (
            UserManagementService userManagementService,
            CancellationToken cancellationToken) =>
        {
            var users = await userManagementService.GetUsersAsync(cancellationToken);
            return Results.Ok(users);
        })
        .WithSummary("Get application users");

        group.MapGet("/roles", () => Results.Ok(AuthRoles.All))
            .WithSummary("Get available user roles");

        group.MapPost("/", async (
            CreateUserRequest request,
            UserManagementService userManagementService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var user = await userManagementService.CreateUserAsync(request, cancellationToken);
                await auditLogService.LogAsync(
                    "users.create",
                    isSuccess: true,
                    entityType: "AppUser",
                    entityId: user.Id.ToString(),
                    details: $"Created user '{user.UserName}' with role '{user.Role}'.",
                    cancellationToken: cancellationToken);

                return Results.Created($"/api/users/{user.Id}", user);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                await auditLogService.LogAsync(
                    "users.create",
                    isSuccess: false,
                    entityType: "AppUser",
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Create application user");

        group.MapPatch("/{userId:guid}/role", async (
            Guid userId,
            ChangeUserRoleRequest request,
            UserManagementService userManagementService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var user = await userManagementService.ChangeRoleAsync(
                    userId,
                    request,
                    cancellationToken);

                if (user is null)
                {
                    await auditLogService.LogAsync(
                        "users.change_role",
                        isSuccess: false,
                        entityType: "AppUser",
                        entityId: userId.ToString(),
                        details: "User not found.",
                        cancellationToken: cancellationToken);

                    return Results.NotFound();
                }

                await auditLogService.LogAsync(
                    "users.change_role",
                    isSuccess: true,
                    entityType: "AppUser",
                    entityId: user.Id.ToString(),
                    details: $"Changed user '{user.UserName}' role to '{user.Role}'.",
                    cancellationToken: cancellationToken);

                return Results.Ok(user);
            }
            catch (Exception exception) when (
                exception is ArgumentException ||
                exception is InvalidOperationException)
            {
                await auditLogService.LogAsync(
                    "users.change_role",
                    isSuccess: false,
                    entityType: "AppUser",
                    entityId: userId.ToString(),
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Change application user role");

        group.MapPatch("/{userId:guid}/status", async (
            Guid userId,
            ChangeUserStatusRequest request,
            ClaimsPrincipal principal,
            UserManagementService userManagementService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetCurrentUserId(principal, out var currentUserId))
                return Results.Unauthorized();

            try
            {
                var user = await userManagementService.ChangeStatusAsync(
                    userId,
                    currentUserId,
                    request,
                    cancellationToken);

                if (user is null)
                {
                    await auditLogService.LogAsync(
                        "users.change_status",
                        isSuccess: false,
                        entityType: "AppUser",
                        entityId: userId.ToString(),
                        details: "User not found.",
                        cancellationToken: cancellationToken);

                    return Results.NotFound();
                }

                await auditLogService.LogAsync(
                    "users.change_status",
                    isSuccess: true,
                    entityType: "AppUser",
                    entityId: user.Id.ToString(),
                    details: $"Changed user '{user.UserName}' status to '{(user.IsEnabled ? "enabled" : "disabled")}'.",
                    cancellationToken: cancellationToken);

                return Results.Ok(user);
            }
            catch (InvalidOperationException exception)
            {
                await auditLogService.LogAsync(
                    "users.change_status",
                    isSuccess: false,
                    entityType: "AppUser",
                    entityId: userId.ToString(),
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Enable or disable application user");

        return app;
    }

    private static bool TryGetCurrentUserId(ClaimsPrincipal principal, out Guid userId)
    {
        return Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}
