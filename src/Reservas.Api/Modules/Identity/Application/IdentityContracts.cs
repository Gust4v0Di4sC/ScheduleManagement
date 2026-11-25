using Reservas.Api.Modules.Identity.Domain;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Identity.Application;

internal interface IUserIdentityStore
{
    Task<UserIdentity?> FindByEmailAsync(
        AccountId? accountId,
        EmailAddress email,
        CancellationToken cancellationToken);

    Task AddAsync(UserIdentity user, CancellationToken cancellationToken);
}

internal interface IRefreshSessionStore
{
    Task<RefreshSession?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task SaveAsync(RefreshSession session, CancellationToken cancellationToken);
}

internal interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}

internal interface IIdentityTokenIssuer
{
    string IssueAccessToken(UserIdentity user, DateTimeOffset now);

    string CreateRefreshToken();

    string HashOpaqueToken(string token);
}

internal interface IIdentityEmailSender
{
    Task SendOwnerInvitationAsync(
        EmailAddress recipient,
        string invitationToken,
        CancellationToken cancellationToken);

    Task SendPasswordRecoveryAsync(
        EmailAddress recipient,
        string recoveryToken,
        CancellationToken cancellationToken);
}
