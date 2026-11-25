using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Identity.Domain;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Identity;

[TestClass]
public sealed class IdentityDomainTests
{
    [TestMethod]
    public void SameEmail_CanCreateUsersInDifferentAccounts()
    {
        var email = EmailAddress.Create("person@example.com");

        var first = UserIdentity.CreateAccountUser(
            new AccountId(Guid.NewGuid()), email, "hash-1", UserRole.Customer);
        var second = UserIdentity.CreateAccountUser(
            new AccountId(Guid.NewGuid()), email, "hash-2", UserRole.Customer);

        first.Email.Should().Be(second.Email);
        first.AccountId.Should().NotBe(second.AccountId);
    }

    [TestMethod]
    public void RefreshSession_ReusingRetiredToken_RevokesSession()
    {
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        var session = new RefreshSession(
            Guid.NewGuid(), Guid.NewGuid(), new AccountId(Guid.NewGuid()), "first", now.AddDays(1));
        session.Rotate("first", "second", now.AddDays(1), now);

        var action = () => session.Rotate("first", "third", now.AddDays(1), now.AddMinutes(1));

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("refresh_token_reused");
        session.RevokedAt.Should().Be(now.AddMinutes(1));
    }

    [TestMethod]
    public void PlatformAdministrator_CannotBeCreatedInsideAccount()
    {
        var action = () => UserIdentity.CreateAccountUser(
            new AccountId(Guid.NewGuid()),
            EmailAddress.Create("admin@example.com"),
            "hash",
            UserRole.PlatformAdministrator);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("platform_admin_has_no_account");
    }
}
