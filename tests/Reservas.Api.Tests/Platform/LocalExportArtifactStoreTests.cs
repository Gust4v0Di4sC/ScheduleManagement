using System.Text;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform;
using Reservas.Api.Modules.Platform.Domain;
using Reservas.Api.Modules.Platform.Infrastructure;

namespace Reservas.Api.Tests.Platform;

[TestClass]
public sealed class LocalExportArtifactStoreTests
{
    private string _temporaryRoot = null!;
    private LocalExportArtifactStore _store = null!;

    [TestInitialize]
    public void Initialize()
    {
        _temporaryRoot = Path.Combine(Path.GetTempPath(), $"reservas-export-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryRoot);
        var options = Options.Create(new PlatformOptions
        {
            LocalStorage = new LocalStorageOptions { RootPath = _temporaryRoot }
        });
        _store = new LocalExportArtifactStore(options, new StubEnvironment(_temporaryRoot));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_temporaryRoot))
        {
            Directory.Delete(_temporaryRoot, true);
        }
    }

    [TestMethod]
    public async Task OpenReadAsync_WithDifferentAccount_DoesNotExposeArtifact()
    {
        var now = DateTimeOffset.UtcNow;
        var owner = new AccountId(Guid.NewGuid());
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("export"));
        var artifact = await _store.SaveAsync(
            owner, "export.json", "application/json", content, now, now.AddHours(1), CancellationToken.None);

        var result = await _store.OpenReadAsync(
            new AccountId(Guid.NewGuid()), artifact.Id, now, CancellationToken.None);

        result.Should().BeNull();
    }

    [TestMethod]
    public async Task DeleteExpiredAsync_RemovesDataAndMetadata()
    {
        var now = DateTimeOffset.UtcNow;
        var accountId = new AccountId(Guid.NewGuid());
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("export"));
        var artifact = await _store.SaveAsync(
            accountId, "export.json", "application/json", content, now, now.AddMinutes(1), CancellationToken.None);

        var deleted = await _store.DeleteExpiredAsync(now.AddMinutes(2), CancellationToken.None);
        var result = await _store.OpenReadAsync(accountId, artifact.Id, now.AddMinutes(2), CancellationToken.None);

        deleted.Should().Be(1);
        result.Should().BeNull();
    }

    private sealed class StubEnvironment(string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = contentRoot;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
