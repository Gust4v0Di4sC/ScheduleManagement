using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reservas.Api.Modules.Platform.Domain;
using Reservas.Api.Modules.Scheduling.Domain;

namespace Reservas.Api.Tests.Scheduling;

[TestClass]
public sealed class AppointmentTests
{
    [TestMethod]
    public void ManualAppointment_RequiresRevalidatedAvailabilityBeforeApproval()
    {
        var appointment = CreateAppointment(AppointmentApprovalMode.Manual);

        var action = () => appointment.Approve(false, DateTimeOffset.UtcNow);

        action.Should().Throw<DomainRuleException>()
            .Which.Code.Should().Be("appointment_unavailable");
        appointment.Status.Should().Be(AppointmentStatus.PendingApproval);
    }

    [TestMethod]
    public void Appointment_StartAndComplete_RecordsActualTimesAndHistory()
    {
        var appointment = CreateAppointment(AppointmentApprovalMode.Automatic);
        var startedAt = new DateTimeOffset(2026, 11, 26, 8, 5, 0, TimeSpan.Zero);

        appointment.Start(startedAt);
        appointment.Complete(startedAt.AddMinutes(50));

        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.ActualStartedAt.Should().Be(startedAt);
        appointment.Events.Select(domainEvent => domainEvent.Type)
            .Should().ContainInOrder("appointment.created", "appointment.approved", "appointment.started", "appointment.completed");
    }

    [TestMethod]
    public void Reschedule_PreservesQuoteWhenNoRecalculationIsProvided_AndEmitsProfessionalChange()
    {
        var appointment = CreateAppointment(AppointmentApprovalMode.Automatic);
        var originalQuote = appointment.Quote;
        var proposed = new TimeRange(appointment.Period.StartsAtUtc.AddDays(1), appointment.Period.EndsAtUtc.AddDays(1));

        appointment.ApplyApprovedReschedule(proposed, Guid.NewGuid(), null, DateTimeOffset.UtcNow);

        appointment.Quote.Should().BeSameAs(originalQuote);
        appointment.Events.Should().Contain(domainEvent => domainEvent.Type == "appointment.professional_changed");
    }

    [TestMethod]
    public void Quote_UsesProfessionalPriceAndLocationFeeWithoutFloatingPoint()
    {
        var quote = QuoteCalculator.Calculate(
            Guid.NewGuid(),
            new Money(100.10m, "BRL"),
            new Money(90.20m, "BRL"),
            new Money(9.80m, "BRL"),
            TimeSpan.FromHours(1),
            DateTimeOffset.UtcNow);

        quote.Total.Should().Be(new Money(100.00m, "BRL"));
        quote.Lines.Should().HaveCount(2);
    }

    private static Appointment CreateAppointment(AppointmentApprovalMode approvalMode)
    {
        var startsAt = new DateTimeOffset(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);
        var quote = QuoteSnapshot.Create(
            Guid.NewGuid(),
            [new QuoteLine("service", "Service", new Money(100, "BRL"))],
            TimeSpan.FromHours(1),
            startsAt.AddDays(-1));
        return Appointment.Create(
            new AccountId(Guid.NewGuid()),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeRange(startsAt, startsAt.AddHours(1)),
            quote,
            approvalMode,
            startsAt.AddDays(-1));
    }
}
