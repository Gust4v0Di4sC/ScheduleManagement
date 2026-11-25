using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform.Domain;
using Reservas.Api.Modules.Professionals.Domain;

namespace Reservas.Api.Tests.Professionals;

[TestClass]
public sealed class ProfessionalDomainTests
{
    [TestMethod]
    public void CanPerform_OnlyEnabledService()
    {
        var serviceId = Guid.NewGuid();
        var professional = new Professional(
            Guid.NewGuid(), new AccountId(Guid.NewGuid()), Guid.NewGuid(), "Professional");

        professional.EnableService(serviceId, null, TimeSpan.FromHours(1));

        professional.CanPerform(serviceId).Should().BeTrue();
        professional.CanPerform(Guid.NewGuid()).Should().BeFalse();
    }

    [TestMethod]
    public void SetWorkPeriods_WithOverlap_IsRejected()
    {
        var professional = new Professional(
            Guid.NewGuid(), new AccountId(Guid.NewGuid()), Guid.NewGuid(), "Professional");

        var action = () => professional.SetWorkPeriods([
            new WeeklyWorkPeriod(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(12, 0)),
            new WeeklyWorkPeriod(DayOfWeek.Monday, new TimeOnly(11, 0), new TimeOnly(13, 0))
        ]);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("professional_schedule_invalid");
    }
}
