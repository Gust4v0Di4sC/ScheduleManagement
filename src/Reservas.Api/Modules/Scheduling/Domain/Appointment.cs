using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Domain;

internal enum AppointmentStatus
{
    PendingApproval,
    Confirmed,
    Rejected,
    Cancelled,
    InProgress,
    Completed,
    Expired
}

internal enum AppointmentApprovalMode
{
    Automatic,
    Manual
}

internal sealed record AppointmentEvent(string Type, DateTimeOffset OccurredAt, string? Reason = null);

internal sealed class Appointment
{
    private readonly List<AppointmentEvent> _events = [];

    private Appointment(
        Guid id,
        AccountId accountId,
        Guid customerId,
        Guid serviceId,
        Guid professionalId,
        TimeRange period,
        QuoteSnapshot quote,
        AppointmentApprovalMode approvalMode,
        DateTimeOffset createdAt)
    {
        Id = id;
        AccountId = accountId;
        CustomerId = customerId;
        ServiceId = serviceId;
        ProfessionalId = professionalId;
        Period = period;
        Quote = quote;
        ApprovalMode = approvalMode;
        Status = approvalMode == AppointmentApprovalMode.Automatic
            ? AppointmentStatus.Confirmed
            : AppointmentStatus.PendingApproval;
        _events.Add(new AppointmentEvent("appointment.created", createdAt));
        if (Status == AppointmentStatus.Confirmed)
        {
            _events.Add(new AppointmentEvent("appointment.approved", createdAt));
        }
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal Guid CustomerId { get; }

    internal Guid ServiceId { get; }

    internal Guid ProfessionalId { get; private set; }

    internal TimeRange Period { get; private set; }

    internal QuoteSnapshot Quote { get; private set; }

    internal AppointmentApprovalMode ApprovalMode { get; }

    internal AppointmentStatus Status { get; private set; }

    internal DateTimeOffset? ActualStartedAt { get; private set; }

    internal DateTimeOffset? CompletedAt { get; private set; }

    internal IReadOnlyCollection<AppointmentEvent> Events => _events.AsReadOnly();

    internal static Appointment Create(
        AccountId accountId,
        Guid customerId,
        Guid serviceId,
        Guid professionalId,
        TimeRange period,
        QuoteSnapshot quote,
        AppointmentApprovalMode approvalMode,
        DateTimeOffset createdAt)
    {
        if (customerId == Guid.Empty || serviceId == Guid.Empty || professionalId == Guid.Empty)
        {
            throw new ArgumentException("Customer, service and professional identifiers are required.");
        }

        ArgumentNullException.ThrowIfNull(quote);
        return new Appointment(
            Guid.NewGuid(), accountId, customerId, serviceId, professionalId, period, quote, approvalMode, createdAt);
    }

    internal void Approve(bool availabilityStillValid, DateTimeOffset occurredAt)
    {
        EnsureStatus(AppointmentStatus.PendingApproval);
        if (!availabilityStillValid)
        {
            throw new DomainRuleException("appointment_unavailable", "Availability must be revalidated before approval.");
        }

        Status = AppointmentStatus.Confirmed;
        _events.Add(new AppointmentEvent("appointment.approved", occurredAt));
    }

    internal void Reject(string reason, DateTimeOffset occurredAt)
    {
        EnsureStatus(AppointmentStatus.PendingApproval);
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = AppointmentStatus.Rejected;
        _events.Add(new AppointmentEvent("appointment.rejected", occurredAt, reason.Trim()));
    }

    internal void Cancel(string reason, DateTimeOffset occurredAt)
    {
        if (Status is not (AppointmentStatus.PendingApproval or AppointmentStatus.Confirmed))
        {
            throw InvalidTransition();
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = AppointmentStatus.Cancelled;
        _events.Add(new AppointmentEvent("appointment.cancelled", occurredAt, reason.Trim()));
    }

    internal void Start(DateTimeOffset occurredAt)
    {
        EnsureStatus(AppointmentStatus.Confirmed);
        Status = AppointmentStatus.InProgress;
        ActualStartedAt = occurredAt;
        _events.Add(new AppointmentEvent("appointment.started", occurredAt));
    }

    internal void Complete(DateTimeOffset occurredAt)
    {
        EnsureStatus(AppointmentStatus.InProgress);
        if (occurredAt < ActualStartedAt)
        {
            throw new DomainRuleException("completion_before_start", "Completion cannot precede the actual start.");
        }

        Status = AppointmentStatus.Completed;
        CompletedAt = occurredAt;
        _events.Add(new AppointmentEvent("appointment.completed", occurredAt));
    }

    internal void Expire(DateTimeOffset occurredAt)
    {
        EnsureStatus(AppointmentStatus.PendingApproval);
        Status = AppointmentStatus.Expired;
        _events.Add(new AppointmentEvent("appointment.expired", occurredAt));
    }

    internal void ApplyApprovedReschedule(
        TimeRange proposedPeriod,
        Guid professionalId,
        QuoteSnapshot? recalculatedQuote,
        DateTimeOffset occurredAt)
    {
        EnsureStatus(AppointmentStatus.Confirmed);
        if (professionalId == Guid.Empty)
        {
            throw new ArgumentException("Professional identifier is required.", nameof(professionalId));
        }

        var professionalChanged = ProfessionalId != professionalId;
        Period = proposedPeriod;
        ProfessionalId = professionalId;
        Quote = recalculatedQuote ?? Quote;
        _events.Add(new AppointmentEvent("appointment.rescheduled", occurredAt));
        if (professionalChanged)
        {
            _events.Add(new AppointmentEvent("appointment.professional_changed", occurredAt));
        }
    }

    private void EnsureStatus(AppointmentStatus expected)
    {
        if (Status != expected)
        {
            throw InvalidTransition();
        }
    }

    private DomainRuleException InvalidTransition() => new(
        "appointment_transition_invalid",
        $"The transition is invalid from status '{Status}'.");
}
