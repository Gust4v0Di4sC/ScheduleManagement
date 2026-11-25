using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Notifications.Application;

namespace Reservas.Api.Tests.Notifications;

[TestClass]
public sealed class CapturedEmailTests
{
    [TestMethod]
    public async Task DevelopmentMailbox_ReturnsCapturedInvitation()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var mailbox = factory.Services.GetRequiredService<ICapturedEmailMailbox>();
        var message = new CapturedEmail(
            Guid.NewGuid(),
            "owner@example.com",
            "Invitation",
            "token",
            "owner-invitation",
            DateTimeOffset.UtcNow);
        await mailbox.CaptureAsync(message, CancellationToken.None);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/development/captured-emails");
        var messages = await response.Content.ReadFromJsonAsync<CapturedEmail[]>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        messages.Should().ContainSingle(candidate => candidate.Id == message.Id);
    }
}
