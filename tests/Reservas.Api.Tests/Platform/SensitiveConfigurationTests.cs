using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform;

namespace Reservas.Api.Tests.Platform;

[TestClass]
[DoNotParallelize]
public sealed class SensitiveConfigurationTests
{
    private const string EnabledVariable = "SensitiveConfiguration__Enabled";
    private const string SecretVariable = "SensitiveConfiguration__PlaceholderApiKey";

    [TestMethod]
    public async Task EnvironmentVariables_ProvideSensitiveConfiguration()
    {
        const string externalSecret = "test-value-from-environment";

        await WithEnvironmentAsync("true", externalSecret, async () =>
        {
            await using var factory = new WebApplicationFactory<Program>();
            using var client = factory.CreateClient();

            var options = factory.Services
                .GetRequiredService<IOptions<SensitiveConfigurationOptions>>()
                .Value;

            options.Enabled.Should().BeTrue();
            options.PlaceholderApiKey.Should().Be(externalSecret);
        });
    }

    [TestMethod]
    public async Task EnabledSensitiveConfiguration_WithoutExternalSecret_RejectsStartup()
    {
        await WithEnvironmentAsync("true", null, () =>
        {
            using var factory = new WebApplicationFactory<Program>();

            Action action = () =>
            {
                using var client = factory.CreateClient();
            };

            action.Should().Throw<OptionsValidationException>()
                .Which.OptionsName.Should().Be(Options.DefaultName);

            return Task.CompletedTask;
        });
    }

    private static async Task WithEnvironmentAsync(
        string? enabled,
        string? secret,
        Func<Task> action)
    {
        var previousEnabled = Environment.GetEnvironmentVariable(EnabledVariable);
        var previousSecret = Environment.GetEnvironmentVariable(SecretVariable);

        try
        {
            Environment.SetEnvironmentVariable(EnabledVariable, enabled);
            Environment.SetEnvironmentVariable(SecretVariable, secret);
            await action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnabledVariable, previousEnabled);
            Environment.SetEnvironmentVariable(SecretVariable, previousSecret);
        }
    }
}
