using System.Text.Json;
using Microsoft.Extensions.Options;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Platform.Infrastructure;

internal sealed class LocalExportArtifactStore : IExportArtifactStore
{
    private readonly string _exportRoot;

    public LocalExportArtifactStore(IOptions<PlatformOptions> options, IHostEnvironment environment)
    {
        var configuredRoot = options.Value.LocalStorage.RootPath;
        var storageRoot = Path.GetFullPath(Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot));
        _exportRoot = Path.Combine(storageRoot, "exports");
    }

    public async Task<ExportArtifact> SaveAsync(
        AccountId accountId,
        string fileName,
        string contentType,
        Stream content,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(content);
        if (expiresAt <= createdAt)
        {
            throw new ArgumentOutOfRangeException(nameof(expiresAt));
        }

        var artifactId = Guid.NewGuid();
        var accountDirectory = Path.Combine(_exportRoot, accountId.Value.ToString("N"));
        Directory.CreateDirectory(accountDirectory);
        var dataPath = Path.Combine(accountDirectory, $"{artifactId:N}.data");
        await using (var destination = new FileStream(
                         dataPath,
                         FileMode.CreateNew,
                         FileAccess.Write,
                         FileShare.None,
                         81_920,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await content.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        }

        var length = new FileInfo(dataPath).Length;
        var artifact = new ExportArtifact(
            artifactId, accountId, Path.GetFileName(fileName), contentType, length, createdAt, expiresAt);
        var metadataPath = GetMetadataPath(accountDirectory, artifactId);
        await using var metadata = new FileStream(
            metadataPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4_096, FileOptions.Asynchronous);
        await JsonSerializer.SerializeAsync(metadata, artifact, cancellationToken: cancellationToken).ConfigureAwait(false);
        return artifact;
    }

    public async Task<Stream?> OpenReadAsync(
        AccountId accountId,
        Guid artifactId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var accountDirectory = Path.Combine(_exportRoot, accountId.Value.ToString("N"));
        var metadataPath = GetMetadataPath(accountDirectory, artifactId);
        if (!File.Exists(metadataPath))
        {
            return null;
        }

        await using var metadata = File.OpenRead(metadataPath);
        var artifact = await JsonSerializer.DeserializeAsync<ExportArtifact>(metadata, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (artifact is null || artifact.AccountId != accountId || artifact.ExpiresAt <= now)
        {
            return null;
        }

        var dataPath = Path.Combine(accountDirectory, $"{artifactId:N}.data");
        return File.Exists(dataPath)
            ? new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81_920, FileOptions.Asynchronous)
            : null;
    }

    public async Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_exportRoot))
        {
            return 0;
        }

        var deleted = 0;
        foreach (var metadataPath in Directory.EnumerateFiles(_exportRoot, "*.json", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                ExportArtifact? artifact;
                await using (var metadata = File.OpenRead(metadataPath))
                {
                    artifact = await JsonSerializer.DeserializeAsync<ExportArtifact>(
                        metadata, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                if (artifact is null || artifact.ExpiresAt > now)
                {
                    continue;
                }

                var directory = Path.GetDirectoryName(metadataPath)!;
                var dataPath = Path.Combine(directory, $"{artifact.Id:N}.data");
                File.Delete(metadataPath);
                if (File.Exists(dataPath))
                {
                    File.Delete(dataPath);
                }

                deleted++;
            }
            catch (JsonException)
            {
                // Corrupt metadata is retained for investigation instead of deleting unknown data.
            }
        }

        return deleted;
    }

    private static string GetMetadataPath(string accountDirectory, Guid artifactId) =>
        Path.Combine(accountDirectory, $"{artifactId:N}.json");
}
