using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Tests.Platform;

[TestClass]
public sealed class OutboxTests
{
    [TestMethod]
    public async Task DispatchAsync_WhenConsumerFails_RecordsFailureForRetry()
    {
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        var message = OutboxMessage.Create(null, "account.created", "{}", now);
        var store = CreateStore(message);
        var inbox = new Mock<IInboxStore>(MockBehavior.Strict);
        inbox.Setup(candidate => candidate.TryBeginAsync(message.Id, "test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var consumer = new Mock<IOutboxMessageConsumer>(MockBehavior.Strict);
        consumer.SetupGet(candidate => candidate.Name).Returns("test");
        consumer.Setup(candidate => candidate.CanConsume("account.created")).Returns(true);
        consumer.Setup(candidate => candidate.ConsumeAsync(message, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("temporary"));
        var dispatcher = CreateDispatcher(store.Object, inbox.Object, consumer.Object, now);

        var processed = await dispatcher.DispatchAsync(10, CancellationToken.None);

        processed.Should().Be(0);
        message.ProcessedAt.Should().BeNull();
        message.Attempts.Should().Be(1);
        message.LastError.Should().Be("temporary");
    }

    [TestMethod]
    public async Task DispatchAsync_WhenInboxAlreadyContainsDelivery_DoesNotRepeatConsumerEffect()
    {
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);
        var message = OutboxMessage.Create(null, "account.created", "{}", now);
        var store = CreateStore(message);
        var inbox = new Mock<IInboxStore>(MockBehavior.Strict);
        inbox.Setup(candidate => candidate.TryBeginAsync(message.Id, "test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var consumer = new Mock<IOutboxMessageConsumer>(MockBehavior.Strict);
        consumer.SetupGet(candidate => candidate.Name).Returns("test");
        consumer.Setup(candidate => candidate.CanConsume("account.created")).Returns(true);
        var dispatcher = CreateDispatcher(store.Object, inbox.Object, consumer.Object, now);

        var processed = await dispatcher.DispatchAsync(10, CancellationToken.None);

        processed.Should().Be(1);
        message.ProcessedAt.Should().Be(now);
        consumer.Verify(
            candidate => candidate.ConsumeAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Mock<IOutboxStore> CreateStore(OutboxMessage message)
    {
        var store = new Mock<IOutboxStore>(MockBehavior.Strict);
        store.Setup(candidate => candidate.GetPendingAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([message]);
        store.Setup(candidate => candidate.RecordFailureAsync(message, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, string, CancellationToken>((outboxMessage, error, _) => outboxMessage.RecordFailure(error))
            .Returns(Task.CompletedTask);
        store.Setup(candidate => candidate.MarkProcessedAsync(message, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, DateTimeOffset, CancellationToken>((outboxMessage, processedAt, _) => outboxMessage.MarkProcessed(processedAt))
            .Returns(Task.CompletedTask);
        return store;
    }

    private static OutboxDispatcher CreateDispatcher(
        IOutboxStore store,
        IInboxStore inbox,
        IOutboxMessageConsumer consumer,
        DateTimeOffset now) =>
        new(store, inbox, [consumer], new StubClock(now), NullLogger<OutboxDispatcher>.Instance);

    private sealed class StubClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
