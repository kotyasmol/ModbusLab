using Microsoft.AspNetCore.SignalR;
using ModbusLab.Api.Realtime;
using ModbusLab.Api.Testing;
using ModbusLab.Application.Testing;

namespace ModbusLab.Api.BackgroundServices;

public sealed class TestRunWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TestRunQueue _queue;
    private readonly IHubContext<ModbusHub> _hubContext;
    private readonly ILogger<TestRunWorker> _logger;

    public TestRunWorker(
        IServiceScopeFactory scopeFactory,
        TestRunQueue queue,
        IHubContext<ModbusHub> hubContext,
        ILogger<TestRunWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var testRunId in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var testExecutionService = scope.ServiceProvider.GetRequiredService<TestExecutionService>();

                await testExecutionService.ExecuteQueuedRunAsync(
                    testRunId,
                    PublishProgressAsync,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to execute queued test run {TestRunId}.", testRunId);
            }
        }
    }

    private Task PublishProgressAsync(
        TestRunProgressEvent progress,
        CancellationToken cancellationToken)
    {
        return _hubContext.Clients.All.SendAsync(
            "TestRunProgress",
            progress,
            cancellationToken);
    }
}
