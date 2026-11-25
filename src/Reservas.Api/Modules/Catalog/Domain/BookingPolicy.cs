using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Catalog.Domain;

internal enum ApprovalMode
{
    Automatic,
    Manual
}

internal enum PaymentMode
{
    AtEstablishment,
    Prepayment,
    Deposit
}

internal sealed record EstablishmentBookingPolicy(
    IReadOnlySet<ApprovalMode> AllowedApprovalModes,
    IReadOnlySet<PaymentMode> AllowedPaymentModes,
    ApprovalMode DefaultApprovalMode,
    PaymentMode DefaultPaymentMode);

internal sealed record ServiceBookingPolicy(
    ApprovalMode ApprovalMode,
    PaymentMode PaymentMode,
    DateTimeOffset EffectiveFrom)
{
    internal static ServiceBookingPolicy Create(
        EstablishmentBookingPolicy establishmentPolicy,
        ApprovalMode? approvalMode,
        PaymentMode? paymentMode,
        DateTimeOffset effectiveFrom)
    {
        ArgumentNullException.ThrowIfNull(establishmentPolicy);
        var selectedApproval = approvalMode ?? establishmentPolicy.DefaultApprovalMode;
        var selectedPayment = paymentMode ?? establishmentPolicy.DefaultPaymentMode;

        if (!establishmentPolicy.AllowedApprovalModes.Contains(selectedApproval) ||
            !establishmentPolicy.AllowedPaymentModes.Contains(selectedPayment))
        {
            throw new DomainRuleException(
                "service_policy_incompatible",
                "The service policy is incompatible with the establishment policy.");
        }

        return new ServiceBookingPolicy(selectedApproval, selectedPayment, effectiveFrom);
    }
}
