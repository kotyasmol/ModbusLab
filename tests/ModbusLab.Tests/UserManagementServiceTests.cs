using ModbusLab.Api.Auth;
using ModbusLab.Domain.Users;
using Xunit;

namespace ModbusLab.Tests;

public sealed class UserManagementServiceTests
{
    [Fact]
    public async Task CreateUserAsync_AdminCanCreateUser()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var service = new UserManagementService(dbContext);

        var user = await service.CreateUserAsync(
            new CreateUserRequest("new-engineer", "new-engineer@local.test", "Password123!", AuthRoles.Engineer),
            CancellationToken.None);

        Assert.Equal("new-engineer", user.UserName);
        Assert.Equal(AuthRoles.Engineer, user.Role);
        Assert.True(user.IsEnabled);
    }

    [Fact]
    public async Task ChangeRoleAsync_ChangesRole()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var service = new UserManagementService(dbContext);
        var user = await service.CreateUserAsync(
            new CreateUserRequest("role-user", null, "Password123!", AuthRoles.Viewer),
            CancellationToken.None);

        var changed = await service.ChangeRoleAsync(
            user.Id,
            new ChangeUserRoleRequest(AuthRoles.Engineer),
            CancellationToken.None);

        Assert.NotNull(changed);
        Assert.Equal(AuthRoles.Engineer, changed.Role);
    }

    [Fact]
    public async Task ChangeRoleAsync_InvalidRoleIsRejected()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var service = new UserManagementService(dbContext);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateUserAsync(
                new CreateUserRequest("bad-role-user", null, "Password123!", "Owner"),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatusAsync_SelfDisableIsRejected()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var service = new UserManagementService(dbContext);
        var admin = await AddUserAsync(dbContext, "admin", AuthRoles.Admin);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeStatusAsync(
                admin.Id,
                admin.Id,
                new ChangeUserStatusRequest(false),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatusAsync_CannotDisableLastActiveAdmin()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var service = new UserManagementService(dbContext);
        var admin = await AddUserAsync(dbContext, "admin", AuthRoles.Admin);
        var operatorUser = await AddUserAsync(dbContext, "operator", AuthRoles.Engineer);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeStatusAsync(
                admin.Id,
                operatorUser.Id,
                new ChangeUserStatusRequest(false),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeRoleAsync_CannotRemoveAdminRoleFromLastActiveAdmin()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var service = new UserManagementService(dbContext);
        var admin = await AddUserAsync(dbContext, "admin", AuthRoles.Admin);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeRoleAsync(
                admin.Id,
                new ChangeUserRoleRequest(AuthRoles.Engineer),
                CancellationToken.None));
    }

    private static async Task<AppUser> AddUserAsync(
        ModbusLab.Infrastructure.Persistence.ModbusLabDbContext dbContext,
        string userName,
        string role)
    {
        var user = new AppUser(userName, $"{userName}@local.test", "__pending__", role);
        user.SetPasswordHash($"hashed-{userName}");

        await dbContext.AppUsers.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return user;
    }
}
