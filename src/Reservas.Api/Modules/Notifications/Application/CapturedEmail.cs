namespace Reservas.Api.Modules.Notifications.Application;

internal sealed record CapturedEmail(
    Guid Id,
    string Recipient,
    string Subject,
    string Body,
    string Purpose,
    DateTimeOffset CapturedAt);

internal interface ICapturedEmailMailbox
{
    Task CaptureAsync(CapturedEmail message, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CapturedEmail>> GetAllAsync(CancellationToken cancellationToken);

    Task ClearAsync(CancellationToken cancellationToken);
}
