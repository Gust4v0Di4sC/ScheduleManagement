using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Accounts.Domain;

internal enum InvitationStatus
{
    Pending,
    Accepted,
    Expired
}

internal sealed class OwnerInvitation
{
    internal OwnerInvitation(
        Guid id,
        AccountId accountId,
        string email,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        Id = id == Guid.Empty ? throw new ArgumentException("Invitation identifier cannot be empty.", nameof(id)) : id;
        AccountId = accountId;
        Email = email.Trim().ToLowerInvariant();
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal string Email { get; }

    internal string TokenHash { get; private set; }

    internal DateTimeOffset ExpiresAt { get; private set; }

    internal InvitationStatus Status { get; private set; }

    internal string? AcceptedTermsVersion { get; private set; }

    internal DateTimeOffset? AcceptedAt { get; private set; }

    internal void Renew(string tokenHash, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        if (Status == InvitationStatus.Accepted)
        {
            throw new DomainRuleException("invitation_already_accepted", "An accepted invitation cannot be renewed.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (expiresAt <= now)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAt), "Invitation expiry must be in the future.");
        }

        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        Status = InvitationStatus.Pending;
    }

    internal void Accept(string termsVersion, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(termsVersion);
        if (Status == InvitationStatus.Accepted)
        {
            throw new DomainRuleException("invitation_already_accepted", "An invitation can only be accepted once.");
        }

        if (now > ExpiresAt)
        {
            Status = InvitationStatus.Expired;
            throw new DomainRuleException("invitation_expired", "The invitation has expired.");
        }

        Status = InvitationStatus.Accepted;
        AcceptedTermsVersion = termsVersion.Trim();
        AcceptedAt = now;
    }
}
