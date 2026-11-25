using Reservas.Api.Modules.Catalog.Application;

namespace Reservas.Api.Modules.Catalog.Infrastructure;

internal sealed class LocalObjectStorage(IConfiguration configuration, IHostEnvironment environment) : IObjectStorage
{
    private readonly string _rootPath = ResolveRootPath(configuration, environment);

    public async Task<StoredObject> StoreAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        var path = ResolveObjectPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var destination = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81_920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await content.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        return new StoredObject(key, contentType, destination.Length, null);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = ResolveObjectPath(key);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolveObjectPath(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var normalizedKey = key.Replace('/', Path.DirectorySeparatorChar);
        var candidate = Path.GetFullPath(Path.Combine(_rootPath, normalizedKey));
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Storage key escapes the configured root.", nameof(key));
        }

        return candidate;
    }

    private static string ResolveRootPath(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["Platform:LocalStorage:RootPath"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException("Platform:LocalStorage:RootPath is required.");
        }

        return Path.GetFullPath(Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured));
    }
}
