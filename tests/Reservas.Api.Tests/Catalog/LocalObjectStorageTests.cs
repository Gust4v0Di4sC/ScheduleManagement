using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Catalog.Infrastructure;

namespace Reservas.Api.Tests.Catalog;

[TestClass]
public sealed class LocalObjectStorageTests
{
    private string _temporaryRoot = null!;

    [TestInitialize]
    public void Initialize()
    {
        _temporaryRoot = Path.Combine(Path.GetTempPath(), $"reservas-storage-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_temporaryRoot);
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
    public async Task StoreAsync_WritesInsideConfiguredRoot()
    {
        var storage = CreateStorage();
        await using var content = new MemoryStream("image"u8.ToArray());

        var stored = await storage.StoreAsync("account/media/file.txt", content, "text/plain", CancellationToken.None);

        stored.Length.Should().Be(5);
        File.Exists(Path.Combine(_temporaryRoot, "account", "media", "file.txt")).Should().BeTrue();
    }

    [TestMethod]
    public async Task StoreAsync_WithTraversalKey_IsRejected()
    {
        var storage = CreateStorage();
        await using var content = new MemoryStream("image"u8.ToArray());

        var action = async () => await storage.StoreAsync("../escape.txt", content, "text/plain", CancellationToken.None);

        await action.Should().ThrowAsync<ArgumentException>();
    }

    private LocalObjectStorage CreateStorage()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Platform:LocalStorage:RootPath"] = _temporaryRoot
            })
            .Build();
        return new LocalObjectStorage(configuration, new StubEnvironment(_temporaryRoot));
    }

    private sealed class StubEnvironment(string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = contentRoot;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
