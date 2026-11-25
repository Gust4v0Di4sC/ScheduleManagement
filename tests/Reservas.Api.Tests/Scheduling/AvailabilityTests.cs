using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform.Domain;
using Reservas.Api.Modules.Scheduling.Domain;

namespace Reservas.Api.Tests.Scheduling;

[TestClass]
public sealed class AvailabilityTests
{
    [TestMethod]
    public void CalculateTimedSlots_ConsidersPreparationCleanupBusyPeriodsAndAbsences()
    {
        var day = new DateTimeOffset(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);
        var professional = new ProfessionalAvailability(
            Guid.NewGuid(),
            [new TimeRange(day, day.AddHours(4))],
            [new TimeRange(day.AddHours(1), day.AddHours(2))],
            [new TimeRange(day.AddHours(3), day.AddHours(4))]);

        var slots = AvailabilityEngine.CalculateTimedSlots(
            [professional],
            TimeSpan.FromHours(1),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromMinutes(15),
            TimeSpan.FromHours(1));

        slots.Should().BeEmpty("buffers make the only otherwise-free slot overlap an occupied period");
    }

    [TestMethod]
    public void SelectProfessionalDeterministically_UsesLowestIdentifier()
    {
        var startsAt = new DateTimeOffset(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);
        var first = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var second = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var period = new TimeRange(startsAt, startsAt.AddHours(1));

        var selected = AvailabilityEngine.SelectProfessionalDeterministically(
            startsAt,
            [new AvailableSlot(second, period), new AvailableSlot(first, period)]);

        selected.Should().Be(first);
    }

    [TestMethod]
    public void EnsureWithinAdvanceWindow_RejectsTooEarlyRequest()
    {
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        var policy = new BookingTimePolicy(TimeSpan.FromHours(2), TimeSpan.FromDays(30), TimeSpan.FromMinutes(15));

        var action = () => TemporalRules.EnsureWithinAdvanceWindow(now.AddHours(1), now, policy);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("booking_outside_advance_window");
    }
}
