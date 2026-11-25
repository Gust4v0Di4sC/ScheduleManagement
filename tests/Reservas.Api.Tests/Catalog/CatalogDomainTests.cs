using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Catalog.Domain;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Catalog;

[TestClass]
public sealed class CatalogDomainTests
{
    [TestMethod]
    public void FixedService_WithCalendarTiming_IsRejected()
    {
        var action = () => new ServiceOffering(
            Guid.NewGuid(),
            new AccountId(Guid.NewGuid()),
            Guid.NewGuid(),
            "Service",
            ServiceModality.FixedDuration,
            ServiceTiming.CalendarBased(),
            new Money(100, "BRL"),
            ServiceLocation.Establishment,
            null);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("service_timing_incompatible");
    }

    [TestMethod]
    public void ServicePolicy_OutsideEstablishmentOptions_IsRejected()
    {
        var establishment = new EstablishmentBookingPolicy(
            new HashSet<ApprovalMode> { ApprovalMode.Manual },
            new HashSet<PaymentMode> { PaymentMode.AtEstablishment },
            ApprovalMode.Manual,
            PaymentMode.AtEstablishment);

        var action = () => ServiceBookingPolicy.Create(
            establishment,
            ApprovalMode.Automatic,
            null,
            DateTimeOffset.UtcNow);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("service_policy_incompatible");
    }

    [TestMethod]
    public void Money_AddDifferentCurrencies_IsRejected()
    {
        var action = () => _ = new Money(10, "BRL") + new Money(2, "USD");

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("currency_mismatch");
    }
}
