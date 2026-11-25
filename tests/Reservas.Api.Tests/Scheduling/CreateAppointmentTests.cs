using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;
using Reservas.Api.Modules.Scheduling.Application;
using Reservas.Api.Modules.Scheduling.Domain;

namespace Reservas.Api.Tests.Scheduling;

[TestClass]
public sealed class CreateAppointmentTests
{
    [TestMethod]
    public async Task HandleAsync_WithCrossAccountReference_RejectsBeforeAvailabilityOrCommit()
    {
        var command = CreateCommand();
        var store = new Mock<IAppointmentStore>(MockBehavior.Strict);
        store.Setup(candidate => candidate.FindByIdempotencyKeyAsync(
                command.AccountId, command.IdempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);
        store.Setup(candidate => candidate.LoadReferencesAsync(
                command.AccountId,
                command.CustomerId,
                command.ServiceId,
                command.ProfessionalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BookingReferences(true, true, false, true));
        var outbox = new Mock<IOutboxStore>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var handler = new CreateAppointmentHandler(
            store.Object,
            outbox.Object,
            unitOfWork.Object,
            new StubClock(DateTimeOffset.UtcNow));

        var action = async () => await handler.HandleAsync(command, CancellationToken.None);

        await action.Should().ThrowAsync<DomainRuleException>()
            .Where(exception => exception.Code == "booking_reference_invalid");
        store.Verify(candidate => candidate.IsAvailableAsync(
            It.IsAny<AccountId>(), It.IsAny<Guid>(), It.IsAny<TimeRange>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(candidate => candidate.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task HandleAsync_WithRepeatedIdempotencyKey_ReturnsExistingAppointmentWithoutWrites()
    {
        var command = CreateCommand();
        var existing = CreateExisting(command);
        var store = new Mock<IAppointmentStore>(MockBehavior.Strict);
        store.Setup(candidate => candidate.FindByIdempotencyKeyAsync(
                command.AccountId, command.IdempotencyKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var handler = new CreateAppointmentHandler(
            store.Object,
            Mock.Of<IOutboxStore>(),
            Mock.Of<IUnitOfWork>(),
            new StubClock(DateTimeOffset.UtcNow));

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().BeSameAs(existing);
        store.Verify(candidate => candidate.AddAsync(
            It.IsAny<Appointment>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static CreateAppointmentCommand CreateCommand()
    {
        var startsAt = new DateTimeOffset(2026, 11, 26, 8, 0, 0, TimeSpan.Zero);
        var quote = QuoteSnapshot.Create(
            Guid.NewGuid(),
            [new QuoteLine("service", "Service", new Money(100, "BRL"))],
            TimeSpan.FromHours(1),
            startsAt.AddDays(-1));
        return new CreateAppointmentCommand(
            new AccountId(Guid.NewGuid()),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new TimeRange(startsAt, startsAt.AddHours(1)),
            quote,
            AppointmentApprovalMode.Manual,
            "idempotency-key");
    }

    private static Appointment CreateExisting(CreateAppointmentCommand command) => Appointment.Create(
        command.AccountId,
        command.CustomerId,
        command.ServiceId,
        command.ProfessionalId,
        command.Period,
        command.Quote,
        command.ApprovalMode,
        DateTimeOffset.UtcNow);

    private sealed class StubClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
