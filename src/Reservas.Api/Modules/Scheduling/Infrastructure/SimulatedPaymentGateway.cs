using System.Collections.Concurrent;
using Reservas.Api.Modules.Scheduling.Application;

namespace Reservas.Api.Modules.Scheduling.Infrastructure;

internal sealed class SimulatedPaymentGateway : IPaymentGateway
{
    private readonly ConcurrentDictionary<string, PaymentGatewayResult> _results = new(StringComparer.Ordinal);

    public Task<PaymentGatewayResult> RegisterAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var result = _results.GetOrAdd(
            request.IdempotencyKey,
            _ => new PaymentGatewayResult($"sim_{Guid.NewGuid():N}", true));
        return Task.FromResult(result);
    }
}
