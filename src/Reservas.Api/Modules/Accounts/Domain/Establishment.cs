using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Accounts.Domain;

internal sealed record WeeklyOpeningPeriod(DayOfWeek Day, TimeOnly StartsAt, TimeOnly EndsAt)
{
    internal TimeSpan Duration => EndsAt - StartsAt;
}

internal sealed class Establishment
{
    private readonly List<WeeklyOpeningPeriod> _openingHours = [];

    internal Establishment(Guid id, AccountId accountId, string name, string segment, string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        Id = id == Guid.Empty ? throw new ArgumentException("Establishment identifier is required.", nameof(id)) : id;
        AccountId = accountId;
        Name = name.Trim();
        Segment = segment.Trim();
        TimeZoneId = timeZoneId;
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal string Name { get; }

    internal string Segment { get; }

    internal string TimeZoneId { get; }

    internal IReadOnlyCollection<WeeklyOpeningPeriod> OpeningHours => _openingHours.AsReadOnly();

    internal void SetOpeningHours(IEnumerable<WeeklyOpeningPeriod> periods)
    {
        ArgumentNullException.ThrowIfNull(periods);
        var ordered = periods.OrderBy(period => period.Day).ThenBy(period => period.StartsAt).ToArray();
        if (ordered.Any(period => period.StartsAt >= period.EndsAt))
        {
            throw new DomainRuleException("opening_hours_invalid", "Opening periods must have a positive duration.");
        }

        var overlaps = ordered.GroupBy(period => period.Day)
            .Any(day => day.Zip(day.Skip(1)).Any(pair => pair.First.EndsAt > pair.Second.StartsAt));
        if (overlaps)
        {
            throw new DomainRuleException("opening_hours_overlap", "Opening periods cannot overlap.");
        }

        _openingHours.Clear();
        _openingHours.AddRange(ordered);
    }
}
