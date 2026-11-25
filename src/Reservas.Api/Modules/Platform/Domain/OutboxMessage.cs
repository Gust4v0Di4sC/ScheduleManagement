namespace Reservas.Api.Modules.Platform.Domain;

internal sealed class OutboxMessage
{
    private OutboxMessage(
        Guid id,
        AccountId? accountId,
        string type,
        string payload,
        DateTimeOffset occurredAt)
    {
        Id = id;
        AccountId = accountId;
        Type = type;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    internal Guid Id { get; }

    internal AccountId? AccountId { get; }

    internal string Type { get; }

    internal string Payload { get; }

    internal DateTimeOffset OccurredAt { get; }

    internal DateTimeOffset? ProcessedAt { get; private set; }

    internal int Attempts { get; private set; }

    internal string? LastError { get; private set; }

    internal static OutboxMessage Create(
        AccountId? accountId,
        string type,
        string payload,
        DateTimeOffset occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        return new OutboxMessage(Guid.NewGuid(), accountId, type, payload, occurredAt);
    }

    internal void RecordFailure(string error)
    {
        if (ProcessedAt.HasValue)
        {
            throw new DomainRuleException("outbox_already_processed", "A processed message cannot fail again.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        Attempts++;
        LastError = error.Length <= 2_000 ? error : error[..2_000];
    }

    internal void MarkProcessed(DateTimeOffset processedAt)
    {
        if (ProcessedAt.HasValue)
        {
            return;
        }

        Attempts++;
        ProcessedAt = processedAt;
        LastError = null;
    }
}
