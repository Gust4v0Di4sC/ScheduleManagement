namespace Reservas.Api.Modules.Platform.Domain;

internal sealed record AuditEntry(
    Guid Id,
    AccountId AccountId,
    Guid ActorId,
    string Action,
    string EntityType,
    string EntityId,
    string IpAddress,
    string CorrelationId,
    string? Justification,
    IReadOnlyDictionary<string, string?> Before,
    IReadOnlyDictionary<string, string?> After,
    DateTimeOffset OccurredAt)
{
    private const string Mask = "***";

    internal static AuditEntry Create(
        AccountId accountId,
        Guid actorId,
        string action,
        string entityType,
        string entityId,
        string ipAddress,
        string correlationId,
        string? justification,
        IReadOnlyDictionary<string, string?> before,
        IReadOnlyDictionary<string, string?> after,
        IReadOnlySet<string> sensitiveFields,
        DateTimeOffset occurredAt)
    {
        if (actorId == Guid.Empty)
        {
            throw new ArgumentException("Actor identifier is required.", nameof(actorId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        return new AuditEntry(
            Guid.NewGuid(), accountId, actorId, action, entityType, entityId, ipAddress, correlationId,
            justification?.Trim(), MaskValues(before, sensitiveFields), MaskValues(after, sensitiveFields), occurredAt);
    }

    private static IReadOnlyDictionary<string, string?> MaskValues(
        IReadOnlyDictionary<string, string?> values,
        IReadOnlySet<string> sensitiveFields) =>
        values.ToDictionary(
            pair => pair.Key,
            pair => sensitiveFields.Contains(pair.Key) && pair.Value is not null ? Mask : pair.Value,
            StringComparer.OrdinalIgnoreCase);
}
