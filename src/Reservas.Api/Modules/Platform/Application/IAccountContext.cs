using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Platform.Application;

internal interface IAccountContext
{
    bool HasAccount { get; }

    AccountId RequireAccountId();
}

internal sealed class CurrentAccountContext : IAccountContext
{
    private AccountId? _accountId;

    public bool HasAccount => _accountId.HasValue;

    internal void Set(AccountId accountId)
    {
        if (_accountId.HasValue && _accountId.Value != accountId)
        {
            throw new InvalidOperationException("The account context cannot change during a request.");
        }

        _accountId = accountId;
    }

    public AccountId RequireAccountId() => _accountId ??
        throw new DomainRuleException("account_context_required", "A valid account context is required.");
}
