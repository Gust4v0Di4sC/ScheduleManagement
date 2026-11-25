using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Accounts.Domain;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Accounts;

[TestClass]
public sealed class AccountsDomainTests
{
    [TestMethod]
    public void Account_OnlyActivatesFromPending()
    {
        var account = Account.Create("Business", DateTimeOffset.UtcNow);

        account.Activate();
        var secondActivation = account.Activate;

        account.Status.Should().Be(AccountStatus.Active);
        secondActivation.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("account_cannot_activate");
    }

    [TestMethod]
    public void Subdomain_NormalizesAndRejectsReservedName()
    {
        var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "admin" };

        var valid = Subdomain.Create("My-Business", reserved);
        var reservedAction = () => Subdomain.Create("ADMIN", reserved);

        valid.Value.Should().Be("my-business");
        reservedAction.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("subdomain_reserved");
    }

    [TestMethod]
    public void Site_PublishWithBlockingReasons_IsRejectedAndReturnsReasons()
    {
        var site = new Site(
            Guid.NewGuid(),
            new AccountId(Guid.NewGuid()),
            Subdomain.Create("business", new HashSet<string>()));
        var readiness = new SitePublicationReadiness(["catalog_required", "schedule_required"]);

        var action = () => site.Publish(readiness, Guid.NewGuid(), DateTimeOffset.UtcNow);

        action.Should().Throw<DomainRuleException>()
            .WithMessage("*catalog_required*schedule_required*");
        site.Status.Should().Be(SiteStatus.Draft);
    }

    [TestMethod]
    public void Invitation_AcceptExpiredInvitation_IsRejected()
    {
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        var invitation = new OwnerInvitation(
            Guid.NewGuid(),
            new AccountId(Guid.NewGuid()),
            "owner@example.com",
            "hash",
            now.AddMinutes(-1));

        var action = () => invitation.Accept("2026-01", now);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("invitation_expired");
        invitation.Status.Should().Be(InvitationStatus.Expired);
    }
}
