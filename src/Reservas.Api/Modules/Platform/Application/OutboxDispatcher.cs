using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Platform.Application;

internal sealed class OutboxDispatcher(
    IOutboxStore outboxStore,
    IInboxStore inboxStore,
    IEnumerable<IOutboxMessageConsumer> consumers,
    IClock clock,
    ILogger<OutboxDispatcher> logger)
{
    internal async Task<int> DispatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        var processed = 0;
        var messages = await outboxStore.GetPendingAsync(batchSize, cancellationToken).ConfigureAwait(false);

        foreach (var message in messages)
        {
            var matchingConsumers = consumers.Where(consumer => consumer.CanConsume(message.Type)).ToArray();
            if (matchingConsumers.Length == 0)
            {
                await outboxStore.RecordFailureAsync(
                    message,
                    $"No consumer is registered for message type '{message.Type}'.",
                    cancellationToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                foreach (var consumer in matchingConsumers)
                {
                    if (!await inboxStore.TryBeginAsync(message.Id, consumer.Name, cancellationToken).ConfigureAwait(false))
                    {
                        continue;
                    }

                    await consumer.ConsumeAsync(message, cancellationToken).ConfigureAwait(false);
                    await inboxStore.MarkCompletedAsync(message.Id, consumer.Name, cancellationToken).ConfigureAwait(false);
                }

                await outboxStore.MarkProcessedAsync(message, clock.UtcNow, cancellationToken).ConfigureAwait(false);
                processed++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Outbox message {MessageId} could not be processed.", message.Id);
                await outboxStore.RecordFailureAsync(message, exception.Message, cancellationToken).ConfigureAwait(false);
            }
        }

        return processed;
    }
}
