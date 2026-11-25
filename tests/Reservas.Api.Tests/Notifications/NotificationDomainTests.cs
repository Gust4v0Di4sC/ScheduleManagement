using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Notifications.Domain;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Notifications;

[TestClass]
public sealed class NotificationDomainTests
{
    [TestMethod]
    public void RenderTemplate_WithMissingValidatedData_IsRejected()
    {
        var template = new NotificationTemplate(
            "invitation",
            "Hello {{name}}",
            "Use {{token}}");

        var action = () => template.RenderBody(new Dictionary<string, string> { ["name"] = "Owner" });

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("notification_template_data_missing");
    }

    [TestMethod]
    public void FailedNotification_CanBeScheduledForRetryAndPreservesAttempt()
    {
        var delivery = new NotificationDelivery(
            Guid.NewGuid(), new AccountId(Guid.NewGuid()), "owner@example.com", "invitation");
        var attemptedAt = DateTimeOffset.UtcNow;

        delivery.RecordTemporaryFailure(attemptedAt, "temporary");
        delivery.ScheduleRetry();

        delivery.Status.Should().Be(NotificationStatus.Pending);
        delivery.Attempts.Should().ContainSingle().Which.Should().Be(
            new NotificationAttempt(attemptedAt, false, "temporary"));
    }
}
