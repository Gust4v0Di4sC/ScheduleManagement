using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Platform;

[TestClass]
public sealed class AccountContextTests
{
    [TestMethod]
    public void RequireAccountId_WithoutContext_RejectsRequest()
    {
        var context = new CurrentAccountContext();

        var action = context.RequireAccountId;

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("account_context_required");
    }

    [TestMethod]
    public void Set_WithDifferentAccount_RejectsContextChange()
    {
        var context = new CurrentAccountContext();
        context.Set(new AccountId(Guid.NewGuid()));

        var action = () => context.Set(new AccountId(Guid.NewGuid()));

        action.Should().Throw<InvalidOperationException>();
    }
}
