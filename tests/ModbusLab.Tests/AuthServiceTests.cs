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

    [Fact]
    public async Task LoginAsync_DisabledUser_ThrowsUnauthorizedAccessException()
    {
        await using var dbContext = CreateDbContext();
        var service = new AuthService(dbContext, CreateConfiguration());
        await service.RegisterAsync(
            new RegisterRequest("disabled-user", null, "Password123!"),
            CancellationToken.None);

        var user = await dbContext.AppUsers.SingleAsync(candidate => candidate.UserName == "disabled-user");
        user.Disable();
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.LoginAsync(
                new LoginRequest("disabled-user", "Password123!"),
                CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_WhenPublicRegistrationDisabled_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var service = new AuthService(
            dbContext,
            CreateConfiguration(new Dictionary<string, string?>
            {
                ["Auth:AllowPublicRegistration"] = "false"
            }));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync(
                new RegisterRequest("blocked-user", null, "Password123!"),
                CancellationToken.None));
    }

    internal static ModbusLabDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ModbusLabDbContext>()
            .UseInMemoryDatabase($"modbuslab-tests-{Guid.NewGuid()}")
            .Options;

        return new ModbusLabDbContext(options);
    }

    internal static IConfiguration CreateConfiguration(
        IReadOnlyDictionary<string, string?>? overrides = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "ModbusLab.Tests",
            ["Jwt:Audience"] = "ModbusLab.Tests",
            ["Jwt:Secret"] = "modbuslab-tests-secret-change-before-production",
            ["Auth:AllowPublicRegistration"] = "true"
        };

        if (overrides is not null)
        {
            foreach (var (key, value) in overrides)
            {
                values[key] = value;
            }
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
