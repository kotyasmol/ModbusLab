using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModbusLab.Api.Auth;
using ModbusLab.Infrastructure.Persistence;
using Xunit;

namespace ModbusLab.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesViewerUserAndReturnsToken()
    {
        await using var dbContext = CreateDbContext();
        var service = new AuthService(dbContext, CreateConfiguration());

        var response = await service.RegisterAsync(
            new RegisterRequest("new-user", "new-user@local.test", "Password123!"),
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.Equal("new-user", response.User.UserName);
        Assert.Equal(AuthRoles.Viewer, response.User.Role);
        Assert.True(await dbContext.AppUsers.AnyAsync(user => user.UserName == "new-user"));
    }

    [Fact]
    public async Task LoginAsync_WithCorrectPassword_ReturnsToken()
    {
        await using var dbContext = CreateDbContext();
        var service = new AuthService(dbContext, CreateConfiguration());
        await service.RegisterAsync(
            new RegisterRequest("login-user", null, "Password123!"),
            CancellationToken.None);

        var response = await service.LoginAsync(
            new LoginRequest("login-user", "Password123!"),
            CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.Equal("login-user", response.User.UserName);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        await using var dbContext = CreateDbContext();
        var service = new AuthService(dbContext, CreateConfiguration());
        await service.RegisterAsync(
            new RegisterRequest("wrong-password-user", null, "Password123!"),
            CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(
                new LoginRequest("wrong-password-user", "WrongPassword123!"),
                CancellationToken.None));
    }

    internal static ModbusLabDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ModbusLabDbContext>()
            .UseInMemoryDatabase($"modbuslab-tests-{Guid.NewGuid()}")
            .Options;

        return new ModbusLabDbContext(options);
    }

    internal static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "ModbusLab.Tests",
                ["Jwt:Audience"] = "ModbusLab.Tests",
                ["Jwt:Secret"] = "modbuslab-tests-secret-change-before-production"
            })
            .Build();
    }
}
