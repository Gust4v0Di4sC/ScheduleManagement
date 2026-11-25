using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Platform.Application;

internal sealed record ExportArtifact(
    Guid Id,
    AccountId AccountId,
    string FileName,
    string ContentType,
    long Length,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

internal interface IExportArtifactStore
{
    Task<ExportArtifact> SaveAsync(
        AccountId accountId,
        string fileName,
        string contentType,
        Stream content,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(
        AccountId accountId,
        Guid artifactId,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken);
}

internal interface IAccountDataExportSource
{
    Task WriteJsonAsync(AccountId accountId, Stream destination, CancellationToken cancellationToken);
}
