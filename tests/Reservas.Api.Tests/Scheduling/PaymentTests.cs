using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform.Domain;
using Reservas.Api.Modules.Scheduling.Application;
using Reservas.Api.Modules.Scheduling.Domain;
using Reservas.Api.Modules.Scheduling.Infrastructure;

namespace Reservas.Api.Tests.Scheduling;

[TestClass]
public sealed class PaymentTests
{
    [TestMethod]
    public async Task SimulatedGateway_RepeatedKeyReturnsSameResult()
    {
        var gateway = new SimulatedPaymentGateway();
        var request = new PaymentGatewayRequest("same-key", new Money(50, "BRL"), "Deposit");

        var first = await gateway.RegisterAsync(request, CancellationToken.None);
        var second = await gateway.RegisterAsync(request, CancellationToken.None);

        second.Should().Be(first);
    }

    [TestMethod]
    public void RegisterPayment_WithZeroAmount_IsRejected()
    {
        var action = () => Payment.Register(
            new AccountId(Guid.NewGuid()),
            Guid.NewGuid(),
            "key",
            PaymentPurpose.FullAmount,
            PaymentMethod.Simulated,
            new Money(0, "BRL"),
            DateTimeOffset.UtcNow);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("payment_amount_invalid");
    }
}
