using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Identity.Domain;

internal sealed class RefreshSession
{
    private readonly HashSet<string> _retiredTokenHashes = new(StringComparer.Ordinal);

    internal RefreshSession(
        Guid id,
        Guid userId,
        AccountId? accountId,
        string tokenHash,
        DateTimeOffset expiresAt)
    {
        if (id == Guid.Empty || userId == Guid.Empty)
        {
            throw new ArgumentException("Session and user identifiers are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        Id = id;
        UserId = userId;
        AccountId = accountId;
        CurrentTokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    internal Guid Id { get; }

    internal Guid UserId { get; }

    internal AccountId? AccountId { get; }

    internal string CurrentTokenHash { get; private set; }

    internal DateTimeOffset ExpiresAt { get; private set; }

    internal DateTimeOffset? RevokedAt { get; private set; }

    internal void Rotate(
        string presentedTokenHash,
        string replacementTokenHash,
        DateTimeOffset replacementExpiresAt,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(presentedTokenHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementTokenHash);

        if (RevokedAt.HasValue)
        {
            throw new DomainRuleException("session_revoked", "The session has been revoked.");
        }

        if (now >= ExpiresAt)
        {
            throw new DomainRuleException("refresh_token_expired", "The refresh token has expired.");
        }

        if (_retiredTokenHashes.Contains(presentedTokenHash))
        {
            RevokedAt = now;
            throw new DomainRuleException("refresh_token_reused", "Refresh-token reuse revoked the session.");
        }

        if (!string.Equals(CurrentTokenHash, presentedTokenHash, StringComparison.Ordinal))
        {
            throw new DomainRuleException("refresh_token_invalid", "The refresh token is invalid.");
        }

        if (replacementExpiresAt <= now)
        {
            throw new ArgumentOutOfRangeException(nameof(replacementExpiresAt));
        }

        _retiredTokenHashes.Add(CurrentTokenHash);
        CurrentTokenHash = replacementTokenHash;
        ExpiresAt = replacementExpiresAt;
    }

    internal void Revoke(DateTimeOffset now)
    {
        RevokedAt ??= now;
    }
}
