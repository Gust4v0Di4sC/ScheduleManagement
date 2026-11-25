using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Notifications.Domain;

internal enum NotificationStatus
{
    Pending,
    Delivered,
    Failed
}

internal sealed record NotificationAttempt(DateTimeOffset AttemptedAt, bool Succeeded, string? Error);

internal sealed class NotificationDelivery
{
    private readonly List<NotificationAttempt> _attempts = [];

    internal NotificationDelivery(Guid id, AccountId accountId, string recipient, string templateKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        Id = id == Guid.Empty ? throw new ArgumentException("Notification identifier is required.", nameof(id)) : id;
        AccountId = accountId;
        Recipient = recipient.Trim();
        TemplateKey = templateKey.Trim();
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal string Recipient { get; }

    internal string TemplateKey { get; }

    internal NotificationStatus Status { get; private set; }

    internal IReadOnlyCollection<NotificationAttempt> Attempts => _attempts.AsReadOnly();

    internal void RecordSuccess(DateTimeOffset attemptedAt)
    {
        if (Status == NotificationStatus.Delivered)
        {
            return;
        }

        _attempts.Add(new NotificationAttempt(attemptedAt, true, null));
        Status = NotificationStatus.Delivered;
    }

    internal void RecordTemporaryFailure(DateTimeOffset attemptedAt, string error)
    {
        if (Status == NotificationStatus.Delivered)
        {
            throw new DomainRuleException("notification_already_delivered", "A delivered notification cannot fail.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(error);
        _attempts.Add(new NotificationAttempt(attemptedAt, false, error));
        Status = NotificationStatus.Failed;
    }

    internal void ScheduleRetry()
    {
        if (Status != NotificationStatus.Failed)
        {
            throw new DomainRuleException("notification_not_failed", "Only a failed notification can be retried.");
        }

        Status = NotificationStatus.Pending;
    }
}
