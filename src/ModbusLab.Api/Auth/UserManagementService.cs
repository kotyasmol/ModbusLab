using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModbusLab.Domain.Users;
using ModbusLab.Infrastructure.Persistence;

namespace ModbusLab.Api.Auth;

public sealed class UserManagementService
{
    private readonly ModbusLabDbContext _dbContext;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public UserManagementService(ModbusLabDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AppUserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.AppUsers
            .AsNoTracking()
            .OrderBy(user => user.UserName)
            .Select(user => new AppUserDto(
                user.Id,
                user.UserName,
                user.Email,
                user.Role,
                user.IsEnabled,
                user.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<AppUserDto> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var userName = NormalizeUserName(request.UserName);
        var email = NormalizeEmail(request.Email);
        var role = ValidateRole(request.Role);

        if (request.Password.Length < 6)
            throw new ArgumentException("Password must contain at least 6 characters.");

        if (await _dbContext.AppUsers.AnyAsync(
                user => user.UserName.ToLower() == userName.ToLower(),
                cancellationToken))
        {
            throw new InvalidOperationException("User name is already taken.");
        }

        if (email is not null && await _dbContext.AppUsers.AnyAsync(
                user => user.Email != null && user.Email.ToLower() == email.ToLower(),
                cancellationToken))
        {
            throw new InvalidOperationException("Email is already taken.");
        }

        var user = new AppUser(userName, email, passwordHash: "__pending__", role);
        user.SetPasswordHash(_passwordHasher.HashPassword(user, request.Password));

        await _dbContext.AppUsers.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<AppUserDto?> ChangeRoleAsync(
        Guid userId,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = ValidateRole(request.Role);
        var user = await _dbContext.AppUsers.FirstOrDefaultAsync(
            candidate => candidate.Id == userId,
            cancellationToken);

        if (user is null)
            return null;

        if (user.IsEnabled && user.Role == AuthRoles.Admin && role != AuthRoles.Admin)
        {
            await EnsureNotLastActiveAdminAsync(user.Id, cancellationToken);
        }

        user.ChangeRole(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public async Task<AppUserDto?> ChangeStatusAsync(
        Guid userId,
        Guid currentUserId,
        ChangeUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.AppUsers.FirstOrDefaultAsync(
            candidate => candidate.Id == userId,
            cancellationToken);

        if (user is null)
            return null;

        if (!request.IsEnabled && user.Id == currentUserId)
            throw new InvalidOperationException("You cannot disable your own account.");

        if (!request.IsEnabled && user.IsEnabled && user.Role == AuthRoles.Admin)
        {
            await EnsureNotLastActiveAdminAsync(user.Id, cancellationToken);
        }

        if (request.IsEnabled)
        {
            user.Enable();
        }
        else
        {
            user.Disable();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(user);
    }

    public static string ValidateRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role is required.");

        var normalizedRole = role.Trim();

        if (!AuthRoles.All.Contains(normalizedRole, StringComparer.Ordinal))
            throw new ArgumentException($"Unknown role '{normalizedRole}'.");

        return normalizedRole;
    }

    public async Task EnsureNotLastActiveAdminAsync(
        Guid adminUserId,
        CancellationToken cancellationToken)
    {
        var otherActiveAdminExists = await _dbContext.AppUsers.AnyAsync(
            user => user.Id != adminUserId &&
                    user.IsEnabled &&
                    user.Role == AuthRoles.Admin,
            cancellationToken);

        if (!otherActiveAdminExists)
            throw new InvalidOperationException("At least one active Admin account must remain.");
    }

    private static AppUserDto ToDto(AppUser user)
    {
        return new AppUserDto(
            user.Id,
            user.UserName,
            user.Email,
            user.Role,
            user.IsEnabled,
            user.CreatedAtUtc);
    }

    private static string NormalizeUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name is required.");

        var normalized = userName.Trim();

        if (normalized.Length < 3)
            throw new ArgumentException("User name must contain at least 3 characters.");

        return normalized;
    }

    private static string? NormalizeEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }
}

public sealed record AppUserDto(
    Guid Id,
    string UserName,
    string? Email,
    string Role,
    bool IsEnabled,
    DateTime CreatedAtUtc);

public sealed record CreateUserRequest(
    string UserName,
    string? Email,
    string Password,
    string Role);

public sealed record ChangeUserRoleRequest(string Role);

public sealed record ChangeUserStatusRequest(bool IsEnabled);
