using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reservas.Api.Modules.Accounts.Application;
using Reservas.Api.Modules.Accounts.Domain;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Accounts;

[TestClass]
public sealed class AccountOnboardingTests
{
    [TestMethod]
    public async Task HandleAsync_StagesAccountContractInvitationAndOutboxBeforeCommit()
    {
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        var sequence = new MockSequence();
        var store = new Mock<IAccountOnboardingStore>(MockBehavior.Strict);
        store.InSequence(sequence).Setup(candidate => candidate.AddAccountAsync(
                It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        store.InSequence(sequence).Setup(candidate => candidate.AddContractAsync(
                It.IsAny<AccountContract>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        store.InSequence(sequence).Setup(candidate => candidate.AddInvitationAsync(
                It.IsAny<OwnerInvitation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var outbox = new Mock<IOutboxStore>(MockBehavior.Strict);
        outbox.InSequence(sequence).Setup(candidate => candidate.AddAsync(
                It.Is<OutboxMessage>(message => message.Type == "owner.invitation.requested"),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unitOfWork.InSequence(sequence).Setup(candidate => candidate.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new CreateAccountAndInviteOwnerHandler(
            store.Object, outbox.Object, unitOfWork.Object, new StubClock(now));
        var command = new CreateAccountAndInviteOwnerCommand(
            "Business",
            PlanId.New(),
            DateOnly.FromDateTime(now.UtcDateTime),
            EmailAddress.Create("owner@example.com"),
            "token-hash",
            now.AddDays(1));

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Account.Status.Should().Be(AccountStatus.Pending);
        result.Contract.AccountId.Should().Be(result.Account.Id);
        result.Invitation.AccountId.Should().Be(result.Account.Id);
        unitOfWork.VerifyAll();
    }

    private sealed class StubClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
