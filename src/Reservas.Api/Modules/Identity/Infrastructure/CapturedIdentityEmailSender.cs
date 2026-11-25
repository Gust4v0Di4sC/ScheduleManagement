using Reservas.Api.Modules.Identity.Application;
using Reservas.Api.Modules.Notifications.Application;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Identity.Infrastructure;

internal sealed class CapturedIdentityEmailSender(
    ICapturedEmailMailbox mailbox,
    IClock clock) : IIdentityEmailSender
{
    public Task SendOwnerInvitationAsync(
        EmailAddress recipient,
        string invitationToken,
        CancellationToken cancellationToken) =>
        CaptureAsync(recipient, invitationToken, "Owner invitation", "owner-invitation", cancellationToken);

    public Task SendPasswordRecoveryAsync(
        EmailAddress recipient,
        string recoveryToken,
        CancellationToken cancellationToken) =>
        CaptureAsync(recipient, recoveryToken, "Password recovery", "password-recovery", cancellationToken);

    private Task CaptureAsync(
        EmailAddress recipient,
        string token,
        string subject,
        string purpose,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return mailbox.CaptureAsync(
            new CapturedEmail(Guid.NewGuid(), recipient.Value, subject, $"Token: {token}", purpose, clock.UtcNow),
            cancellationToken);
    }
}
