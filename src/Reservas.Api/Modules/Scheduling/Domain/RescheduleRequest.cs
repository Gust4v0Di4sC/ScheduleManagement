using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Domain;

internal enum RescheduleStatus
{
    Pending,
    Approved,
    Rejected
}

internal sealed class RescheduleRequest
{
    internal RescheduleRequest(
        Guid id,
        Guid appointmentId,
        AccountId accountId,
        TimeRange originalPeriod,
        TimeRange proposedPeriod,
        DateTimeOffset requestedAt)
    {
        Id = id == Guid.Empty ? throw new ArgumentException("Request identifier is required.", nameof(id)) : id;
        AppointmentId = appointmentId == Guid.Empty
            ? throw new ArgumentException("Appointment identifier is required.", nameof(appointmentId))
            : appointmentId;
        AccountId = accountId;
        OriginalPeriod = originalPeriod;
        ProposedPeriod = proposedPeriod;
        RequestedAt = requestedAt;
    }

    internal Guid Id { get; }

    internal Guid AppointmentId { get; }

    internal AccountId AccountId { get; }

    internal TimeRange OriginalPeriod { get; }

    internal TimeRange ProposedPeriod { get; }

    internal DateTimeOffset RequestedAt { get; }

    internal RescheduleStatus Status { get; private set; }

    internal string? DecisionReason { get; private set; }

    internal void Approve(bool availabilityStillValid)
    {
        EnsurePending();
        if (!availabilityStillValid)
        {
            throw new DomainRuleException("reschedule_unavailable", "The proposed period is no longer available.");
        }

        Status = RescheduleStatus.Approved;
    }

    internal void Reject(string reason)
    {
        EnsurePending();
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        DecisionReason = reason.Trim();
        Status = RescheduleStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != RescheduleStatus.Pending)
        {
            throw new DomainRuleException("reschedule_already_decided", "The reschedule request was already decided.");
        }
    }
}
