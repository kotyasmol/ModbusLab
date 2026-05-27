using System.Threading.Channels;

namespace ModbusLab.Api.Testing;

public sealed class TestRunQueue
{
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>();

    public ValueTask EnqueueAsync(Guid testRunId, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(testRunId, cancellationToken);
    }

    public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }
}
