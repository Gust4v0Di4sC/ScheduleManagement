using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Domain;

internal readonly record struct TimeRange
{
    internal TimeRange(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        if (startsAtUtc.Offset != TimeSpan.Zero || endsAtUtc.Offset != TimeSpan.Zero)
        {
            throw new DomainRuleException("utc_required", "Scheduling instants must use the UTC offset.");
        }

        if (endsAtUtc <= startsAtUtc)
        {
            throw new DomainRuleException("time_range_invalid", "The end must be later than the start.");
        }

        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    internal DateTimeOffset StartsAtUtc { get; }

    internal DateTimeOffset EndsAtUtc { get; }

    internal TimeSpan Duration => EndsAtUtc - StartsAtUtc;

    internal bool Overlaps(TimeRange other) =>
        StartsAtUtc < other.EndsAtUtc && other.StartsAtUtc < EndsAtUtc;

    internal TimeRange Expand(TimeSpan before, TimeSpan after)
    {
        if (before < TimeSpan.Zero || after < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(before), "Buffers cannot be negative.");
        }

        return new TimeRange(StartsAtUtc - before, EndsAtUtc + after);
    }
}
