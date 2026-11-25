using System.Text.Json;
using Reservas.Api.Modules.Accounts.Domain;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Accounts.Application;

internal sealed record CreateAccountAndInviteOwnerCommand(
    string AccountName,
    PlanId PlanId,
    DateOnly ContractStartsOn,
    EmailAddress OwnerEmail,
    string InvitationTokenHash,
    DateTimeOffset InvitationExpiresAt);

internal sealed record AccountOnboardingResult(Account Account, AccountContract Contract, OwnerInvitation Invitation);

internal interface IAccountOnboardingStore
{
    Task AddAccountAsync(Account account, CancellationToken cancellationToken);

    Task AddContractAsync(AccountContract contract, CancellationToken cancellationToken);

    Task AddInvitationAsync(OwnerInvitation invitation, CancellationToken cancellationToken);
}

internal sealed class CreateAccountAndInviteOwnerHandler(
    IAccountOnboardingStore store,
    IOutboxStore outboxStore,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    internal async Task<AccountOnboardingResult> HandleAsync(
        CreateAccountAndInviteOwnerCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var account = Account.Create(command.AccountName, clock.UtcNow);
        var contract = new AccountContract(
            Guid.NewGuid(), account.Id, command.PlanId, command.ContractStartsOn, null);
        var invitation = new OwnerInvitation(
            Guid.NewGuid(),
            account.Id,
            command.OwnerEmail.Value,
            command.InvitationTokenHash,
            command.InvitationExpiresAt);

        await store.AddAccountAsync(account, cancellationToken).ConfigureAwait(false);
        await store.AddContractAsync(contract, cancellationToken).ConfigureAwait(false);
        await store.AddInvitationAsync(invitation, cancellationToken).ConfigureAwait(false);
        await outboxStore.AddAsync(
            OutboxMessage.Create(
                account.Id,
                "owner.invitation.requested",
                JsonSerializer.Serialize(new { InvitationId = invitation.Id, Recipient = invitation.Email }),
                clock.UtcNow),
            cancellationToken).ConfigureAwait(false);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new AccountOnboardingResult(account, contract, invitation);
    }
}
