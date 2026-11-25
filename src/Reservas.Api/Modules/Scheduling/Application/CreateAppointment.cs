using System.Text.Json;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;
using Reservas.Api.Modules.Scheduling.Domain;

namespace Reservas.Api.Modules.Scheduling.Application;

internal sealed record CreateAppointmentCommand(
    AccountId AccountId,
    Guid CustomerId,
    Guid ServiceId,
    Guid ProfessionalId,
    TimeRange Period,
    QuoteSnapshot Quote,
    AppointmentApprovalMode ApprovalMode,
    string IdempotencyKey);

internal sealed record BookingReferences(
    bool CustomerBelongsToAccount,
    bool ServiceBelongsToAccount,
    bool ProfessionalBelongsToAccount,
    bool ProfessionalCanPerformService)
{
    internal bool AreValid => CustomerBelongsToAccount && ServiceBelongsToAccount &&
                              ProfessionalBelongsToAccount && ProfessionalCanPerformService;
}

internal interface IAppointmentStore
{
    Task<Appointment?> FindByIdempotencyKeyAsync(
        AccountId accountId,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<BookingReferences> LoadReferencesAsync(
        AccountId accountId,
        Guid customerId,
        Guid serviceId,
        Guid professionalId,
        CancellationToken cancellationToken);

    Task<bool> IsAvailableAsync(
        AccountId accountId,
        Guid professionalId,
        TimeRange period,
        CancellationToken cancellationToken);

    Task AddAsync(Appointment appointment, string idempotencyKey, CancellationToken cancellationToken);
}

internal sealed class CreateAppointmentHandler(
    IAppointmentStore store,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    internal async Task<Appointment> HandleAsync(
        CreateAppointmentCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(command.IdempotencyKey);
        var existing = await store.FindByIdempotencyKeyAsync(
            command.AccountId, command.IdempotencyKey, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return existing;
        }

        var references = await store.LoadReferencesAsync(
            command.AccountId,
            command.CustomerId,
            command.ServiceId,
            command.ProfessionalId,
            cancellationToken).ConfigureAwait(false);
        if (!references.AreValid)
        {
            throw new DomainRuleException(
                "booking_reference_invalid",
                "Customer, service and professional must belong to the current account and be compatible.");
        }

        if (!await store.IsAvailableAsync(
                command.AccountId, command.ProfessionalId, command.Period, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new DomainRuleException("appointment_unavailable", "The requested period is unavailable.");
        }

        var appointment = Appointment.Create(
            command.AccountId,
            command.CustomerId,
            command.ServiceId,
            command.ProfessionalId,
            command.Period,
            command.Quote,
            command.ApprovalMode,
            clock.UtcNow);
        await store.AddAsync(appointment, command.IdempotencyKey, cancellationToken).ConfigureAwait(false);
        await outboxStore.AddAsync(
            OutboxMessage.Create(
                command.AccountId,
                "appointment.created",
                JsonSerializer.Serialize(new { AppointmentId = appointment.Id }),
                clock.UtcNow),
            cancellationToken).ConfigureAwait(false);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return appointment;
    }
}
