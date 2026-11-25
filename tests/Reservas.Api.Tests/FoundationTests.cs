using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reservas.Api.Tests;

[TestClass]
public sealed class FoundationTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [TestInitialize]
    public void Initialize() => _factory = new WebApplicationFactory<Program>();

    [TestCleanup]
    public async Task CleanupAsync() => await _factory.DisposeAsync();

    [TestMethod]
    public async Task HealthLive_ReturnsHealthyAndSecurityHeaders()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
        response.Headers.Should().ContainKey("X-Correlation-ID");
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-Frame-Options");
    }

    [TestMethod]
    public async Task OpenApi_IsAvailableAndDoesNotContainTemplateEndpoint()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/openapi/v1.json");
        var document = await response.Content.ReadFromJsonAsync<OpenApiDocument>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        document.Should().NotBeNull();
        document!.Paths.Keys.Should().NotContain(path => path.Contains("WeatherForecast", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record OpenApiDocument(Dictionary<string, object> Paths);
}
