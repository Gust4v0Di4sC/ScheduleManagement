using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Domain;

internal sealed record BookingTimePolicy(
    TimeSpan MinimumAdvance,
    TimeSpan MaximumAdvance,
    TimeSpan GridSize)
{
    internal void Validate()
    {
        if (MinimumAdvance < TimeSpan.Zero || MaximumAdvance <= MinimumAdvance || GridSize <= TimeSpan.Zero)
        {
            throw new DomainRuleException("booking_time_policy_invalid", "Booking time policy values are invalid.");
        }
    }
}

internal static class TemporalRules
{
    internal static DateTimeOffset LocalToUtc(DateTime localDateTime, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(unspecified))
        {
            throw new DomainRuleException("local_time_invalid", "The local time does not exist in the selected time zone.");
        }

        var offset = timeZone.IsAmbiguousTime(unspecified)
            ? timeZone.GetAmbiguousTimeOffsets(unspecified).Max()
            : timeZone.GetUtcOffset(unspecified);
        return new DateTimeOffset(unspecified, offset).ToUniversalTime();
    }

    internal static void EnsureWithinAdvanceWindow(
        DateTimeOffset startsAtUtc,
        DateTimeOffset nowUtc,
        BookingTimePolicy policy)
    {
        policy.Validate();
        var advance = startsAtUtc - nowUtc;
        if (advance < policy.MinimumAdvance || advance > policy.MaximumAdvance)
        {
            throw new DomainRuleException(
                "booking_outside_advance_window",
                "The requested time is outside the allowed booking window.");
        }
    }
}
