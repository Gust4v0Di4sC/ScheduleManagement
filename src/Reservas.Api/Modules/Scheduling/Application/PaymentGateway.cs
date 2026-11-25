using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Application;

internal sealed record PaymentGatewayRequest(
    string IdempotencyKey,
    Money Amount,
    string Description);

internal sealed record PaymentGatewayResult(string ExternalId, bool Approved);

internal interface IPaymentGateway
{
    Task<PaymentGatewayResult> RegisterAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken);
}
