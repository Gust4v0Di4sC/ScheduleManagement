using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Professionals.Domain;

internal sealed record ProfessionalService(
    Guid ServiceId,
    Money? CustomPrice,
    TimeSpan? CustomDuration);

internal sealed record WeeklyWorkPeriod(DayOfWeek Day, TimeOnly StartsAt, TimeOnly EndsAt);

internal sealed class Professional
{
    private readonly Dictionary<Guid, ProfessionalService> _services = [];
    private readonly List<WeeklyWorkPeriod> _workPeriods = [];

    internal Professional(Guid id, AccountId accountId, Guid establishmentId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id == Guid.Empty ? throw new ArgumentException("Professional identifier is required.", nameof(id)) : id;
        EstablishmentId = establishmentId == Guid.Empty
            ? throw new ArgumentException("Establishment identifier is required.", nameof(establishmentId))
            : establishmentId;
        AccountId = accountId;
        Name = name.Trim();
        IsActive = true;
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal Guid EstablishmentId { get; }

    internal string Name { get; }

    internal bool IsActive { get; private set; }

    internal IReadOnlyCollection<ProfessionalService> Services => _services.Values;

    internal IReadOnlyCollection<WeeklyWorkPeriod> WorkPeriods => _workPeriods.AsReadOnly();

    internal void EnableService(Guid serviceId, Money? customPrice, TimeSpan? customDuration)
    {
        if (serviceId == Guid.Empty)
        {
            throw new ArgumentException("Service identifier is required.", nameof(serviceId));
        }

        if (customDuration.HasValue && customDuration.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(customDuration));
        }

        _services[serviceId] = new ProfessionalService(serviceId, customPrice, customDuration);
    }

    internal bool CanPerform(Guid serviceId) => IsActive && _services.ContainsKey(serviceId);

    internal void SetWorkPeriods(IEnumerable<WeeklyWorkPeriod> periods)
    {
        ArgumentNullException.ThrowIfNull(periods);
        var ordered = periods.OrderBy(period => period.Day).ThenBy(period => period.StartsAt).ToArray();
        if (ordered.Any(period => period.StartsAt >= period.EndsAt) ||
            ordered.GroupBy(period => period.Day)
                .Any(day => day.Zip(day.Skip(1)).Any(pair => pair.First.EndsAt > pair.Second.StartsAt)))
        {
            throw new DomainRuleException("professional_schedule_invalid", "Professional work periods are invalid.");
        }

        _workPeriods.Clear();
        _workPeriods.AddRange(ordered);
    }
}
