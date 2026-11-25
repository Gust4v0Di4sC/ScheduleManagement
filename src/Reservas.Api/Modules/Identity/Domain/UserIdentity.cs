using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Identity.Domain;

internal enum UserRole
{
    PlatformAdministrator,
    Owner,
    Customer
}

internal enum UserStatus
{
    Invited,
    Active,
    Disabled
}

internal sealed class UserIdentity
{
    private UserIdentity(
        Guid id,
        AccountId? accountId,
        EmailAddress email,
        string passwordHash,
        UserRole role,
        UserStatus status)
    {
        Id = id;
        AccountId = accountId;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        Status = status;
    }

    internal Guid Id { get; }

    internal AccountId? AccountId { get; }

    internal EmailAddress Email { get; }

    internal string PasswordHash { get; private set; }

    internal UserRole Role { get; }

    internal UserStatus Status { get; private set; }

    internal static UserIdentity CreateAccountUser(
        AccountId accountId,
        EmailAddress email,
        string passwordHash,
        UserRole role,
        bool active = true)
    {
        if (role == UserRole.PlatformAdministrator)
        {
            throw new DomainRuleException(
                "platform_admin_has_no_account",
                "A platform administrator must be created outside an account context.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        return new UserIdentity(
            Guid.NewGuid(),
            accountId,
            email,
            passwordHash,
            role,
            active ? UserStatus.Active : UserStatus.Invited);
    }

    internal static UserIdentity CreatePlatformAdministrator(EmailAddress email, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        return new UserIdentity(
            Guid.NewGuid(),
            null,
            email,
            passwordHash,
            UserRole.PlatformAdministrator,
            UserStatus.Active);
    }

    internal void Activate(string passwordHash)
    {
        if (Status != UserStatus.Invited)
        {
            throw new DomainRuleException("user_not_invited", "Only an invited user can be activated.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        Status = UserStatus.Active;
    }

    internal void Disable() => Status = UserStatus.Disabled;
}
