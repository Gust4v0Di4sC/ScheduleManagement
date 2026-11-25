using System.Collections.Concurrent;
using Reservas.Api.Modules.Notifications.Application;

namespace Reservas.Api.Modules.Notifications.Infrastructure;

internal sealed class InMemoryCapturedEmailMailbox : ICapturedEmailMailbox
{
    private readonly ConcurrentQueue<CapturedEmail> _messages = new();

    public Task CaptureAsync(CapturedEmail message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        _messages.Enqueue(message);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CapturedEmail>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<CapturedEmail>>(_messages.ToArray());
    }

    public Task ClearAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _messages.Clear();
        return Task.CompletedTask;
    }
}
