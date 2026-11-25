using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Domain;

internal enum PaymentPurpose
{
    FullAmount,
    Deposit,
    Difference
}

internal enum PaymentMethod
{
    Cash,
    Pix,
    Card,
    Simulated
}

internal sealed record Payment(
    Guid Id,
    AccountId AccountId,
    Guid AppointmentId,
    string IdempotencyKey,
    PaymentPurpose Purpose,
    PaymentMethod Method,
    Money Amount,
    DateTimeOffset RegisteredAt)
{
    internal static Payment Register(
        AccountId accountId,
        Guid appointmentId,
        string idempotencyKey,
        PaymentPurpose purpose,
        PaymentMethod method,
        Money amount,
        DateTimeOffset registeredAt)
    {
        if (appointmentId == Guid.Empty)
        {
            throw new ArgumentException("Appointment identifier is required.", nameof(appointmentId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (amount.Amount <= 0)
        {
            throw new DomainRuleException("payment_amount_invalid", "Payment amount must be positive.");
        }

        return new Payment(
            Guid.NewGuid(), accountId, appointmentId, idempotencyKey.Trim(), purpose, method, amount, registeredAt);
    }
}
