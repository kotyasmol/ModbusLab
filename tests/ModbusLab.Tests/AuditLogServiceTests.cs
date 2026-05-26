using Microsoft.AspNetCore.Http;
using ModbusLab.Api.Audit;
using Xunit;

namespace ModbusLab.Tests;

public sealed class AuditLogServiceTests
{
    [Fact]
    public async Task LogAsync_CreatesAuditLogEntry()
    {
        await using var dbContext = AuthServiceTests.CreateDbContext();
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var service = new AuditLogService(dbContext, httpContextAccessor);

        await service.LogAsync(
            "tests.audit",
            isSuccess: true,
            details: "Audit test");

        var entry = Assert.Single(dbContext.AuditLogs);
        Assert.Equal("tests.audit", entry.Action);
        Assert.True(entry.IsSuccess);
    }
}
