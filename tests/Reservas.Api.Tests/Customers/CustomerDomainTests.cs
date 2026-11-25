using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Customers.Domain;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Customers;

[TestClass]
public sealed class CustomerDomainTests
{
    [TestMethod]
    public void RecordConsent_PreservesVersionOriginAndDate()
    {
        var acceptedAt = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        var customer = new Customer(
            Guid.NewGuid(),
            new AccountId(Guid.NewGuid()),
            "Customer",
            EmailAddress.Create("customer@example.com"),
            BrazilianTaxId.Create("529.982.247-25"));

        customer.RecordConsent("privacy-2026-01", "public-site", acceptedAt);

        customer.Consents.Should().ContainSingle().Which.Should().Be(
            new ConsentRecord("privacy-2026-01", "public-site", acceptedAt));
    }

    [TestMethod]
    public void CreateCpf_WithInvalidCheckDigits_IsRejected()
    {
        var action = () => BrazilianTaxId.Create("123.456.789-00");

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("cpf_invalid");
    }
}
