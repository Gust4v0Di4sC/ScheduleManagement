using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Platform.Application;

internal interface IOutboxStore
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken);

    Task MarkProcessedAsync(OutboxMessage message, DateTimeOffset processedAt, CancellationToken cancellationToken);

    Task RecordFailureAsync(OutboxMessage message, string error, CancellationToken cancellationToken);
}

internal interface IInboxStore
{
    Task<bool> TryBeginAsync(Guid messageId, string consumerName, CancellationToken cancellationToken);

    Task MarkCompletedAsync(Guid messageId, string consumerName, CancellationToken cancellationToken);
}

internal interface IOutboxMessageConsumer
{
    string Name { get; }

    bool CanConsume(string messageType);

    Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken);
}
