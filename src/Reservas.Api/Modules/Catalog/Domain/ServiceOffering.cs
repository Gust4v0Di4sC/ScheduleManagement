using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Catalog.Domain;

internal enum ServiceModality
{
    FixedDuration,
    VariableDuration,
    FullDay,
    Daily
}

internal enum ServiceLocation
{
    Establishment,
    CustomerAddress,
    Both
}

internal sealed record ServiceTiming(
    TimeSpan? FixedDuration,
    TimeSpan? MinimumDuration,
    TimeSpan? MaximumDuration)
{
    internal static ServiceTiming Fixed(TimeSpan duration)
    {
        EnsurePositive(duration, nameof(duration));
        return new ServiceTiming(duration, null, null);
    }

    internal static ServiceTiming Variable(TimeSpan minimumDuration, TimeSpan maximumDuration)
    {
        EnsurePositive(minimumDuration, nameof(minimumDuration));
        EnsurePositive(maximumDuration, nameof(maximumDuration));
        if (maximumDuration < minimumDuration)
        {
            throw new DomainRuleException(
                "service_duration_range_invalid",
                "Maximum duration cannot be shorter than minimum duration.");
        }

        return new ServiceTiming(null, minimumDuration, maximumDuration);
    }

    internal static ServiceTiming CalendarBased() => new(null, null, null);

    private static void EnsurePositive(TimeSpan duration, string parameterName)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Duration must be positive.");
        }
    }
}

internal sealed class ServiceOffering
{
    internal ServiceOffering(
        Guid id,
        AccountId accountId,
        Guid categoryId,
        string name,
        ServiceModality modality,
        ServiceTiming timing,
        Money basePrice,
        ServiceLocation location,
        Money? customerLocationFee)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateTiming(modality, timing);
        if (location == ServiceLocation.Establishment && customerLocationFee.HasValue)
        {
            throw new DomainRuleException(
                "location_fee_not_allowed",
                "A customer-location fee is not allowed for establishment-only services.");
        }

        Id = id == Guid.Empty ? throw new ArgumentException("Service identifier is required.", nameof(id)) : id;
        AccountId = accountId;
        CategoryId = categoryId;
        Name = name.Trim();
        Modality = modality;
        Timing = timing;
        BasePrice = basePrice;
        Location = location;
        CustomerLocationFee = customerLocationFee;
        IsActive = true;
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal Guid CategoryId { get; }

    internal string Name { get; }

    internal ServiceModality Modality { get; }

    internal ServiceTiming Timing { get; }

    internal Money BasePrice { get; }

    internal ServiceLocation Location { get; }

    internal Money? CustomerLocationFee { get; }

    internal bool IsActive { get; private set; }

    internal void Activate() => IsActive = true;

    internal void Deactivate() => IsActive = false;

    private static void ValidateTiming(ServiceModality modality, ServiceTiming timing)
    {
        ArgumentNullException.ThrowIfNull(timing);
        var valid = modality switch
        {
            ServiceModality.FixedDuration => timing.FixedDuration.HasValue &&
                                             !timing.MinimumDuration.HasValue &&
                                             !timing.MaximumDuration.HasValue,
            ServiceModality.VariableDuration => !timing.FixedDuration.HasValue &&
                                                timing.MinimumDuration.HasValue &&
                                                timing.MaximumDuration.HasValue,
            ServiceModality.FullDay or ServiceModality.Daily => !timing.FixedDuration.HasValue &&
                                                               !timing.MinimumDuration.HasValue &&
                                                               !timing.MaximumDuration.HasValue,
            _ => false
        };

        if (!valid)
        {
            throw new DomainRuleException(
                "service_timing_incompatible",
                "The timing fields are incompatible with the service modality.");
        }
    }
}
