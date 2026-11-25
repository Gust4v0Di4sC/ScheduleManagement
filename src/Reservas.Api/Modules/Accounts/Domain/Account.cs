using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Accounts.Domain;

internal enum AccountStatus
{
    Pending,
    Active,
    Suspended
}

internal sealed class Account
{
    private Account(AccountId id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
        Status = AccountStatus.Pending;
    }

    internal AccountId Id { get; }

    internal string Name { get; private set; }

    internal AccountStatus Status { get; private set; }

    internal DateTimeOffset CreatedAt { get; }

    internal static Account Create(string name, DateTimeOffset createdAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalizedName = name.Trim();
        if (normalizedName.Length > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(name), "Account name cannot exceed 200 characters.");
        }

        return new Account(new AccountId(Guid.NewGuid()), normalizedName, createdAt);
    }

    internal void Activate()
    {
        if (Status != AccountStatus.Pending)
        {
            throw new DomainRuleException("account_cannot_activate", "Only a pending account can be activated.");
        }

        Status = AccountStatus.Active;
    }

    internal void Suspend()
    {
        if (Status != AccountStatus.Active)
        {
            throw new DomainRuleException("account_cannot_suspend", "Only an active account can be suspended.");
        }

        Status = AccountStatus.Suspended;
    }
}
