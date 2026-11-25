namespace Reservas.Api.Modules.Catalog.Application;

internal sealed record StoredObject(string Key, string ContentType, long Length, Uri? PublicUri);

internal interface IObjectStorage
{
    Task<StoredObject> StoreAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken);

    Task DeleteAsync(string key, CancellationToken cancellationToken);
}
