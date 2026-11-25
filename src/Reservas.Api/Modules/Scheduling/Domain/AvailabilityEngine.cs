using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Domain;

internal sealed record ProfessionalAvailability(
    Guid ProfessionalId,
    IReadOnlyCollection<TimeRange> WorkPeriods,
    IReadOnlyCollection<TimeRange> BusyPeriods,
    IReadOnlyCollection<TimeRange> Absences);

internal sealed record AvailableSlot(Guid ProfessionalId, TimeRange Period);

internal static class AvailabilityEngine
{
    internal static IReadOnlyCollection<AvailableSlot> CalculateTimedSlots(
        IEnumerable<ProfessionalAvailability> professionals,
        TimeSpan serviceDuration,
        TimeSpan preparation,
        TimeSpan cleanup,
        TimeSpan gridSize)
    {
        ArgumentNullException.ThrowIfNull(professionals);
        if (serviceDuration <= TimeSpan.Zero || gridSize <= TimeSpan.Zero ||
            preparation < TimeSpan.Zero || cleanup < TimeSpan.Zero)
        {
            throw new DomainRuleException("availability_configuration_invalid", "Availability durations are invalid.");
        }

        var slots = new List<AvailableSlot>();
        foreach (var professional in professionals.OrderBy(candidate => candidate.ProfessionalId))
        {
            foreach (var window in professional.WorkPeriods.OrderBy(period => period.StartsAtUtc))
            {
                for (var start = window.StartsAtUtc; start + serviceDuration <= window.EndsAtUtc; start += gridSize)
                {
                    var servicePeriod = new TimeRange(start, start + serviceDuration);
                    var occupiedPeriod = servicePeriod.Expand(preparation, cleanup);
                    if (professional.BusyPeriods.Any(occupiedPeriod.Overlaps) ||
                        professional.Absences.Any(occupiedPeriod.Overlaps))
                    {
                        continue;
                    }

                    slots.Add(new AvailableSlot(professional.ProfessionalId, servicePeriod));
                }
            }
        }

        return slots;
    }

    internal static IReadOnlyCollection<DateOnly> CalculateAvailableDays(
        DateOnly startsOn,
        DateOnly endsOn,
        IReadOnlySet<DateOnly> unavailableDays)
    {
        if (endsOn < startsOn)
        {
            throw new ArgumentException("End date cannot precede start date.", nameof(endsOn));
        }

        var result = new List<DateOnly>();
        for (var day = startsOn; day <= endsOn; day = day.AddDays(1))
        {
            if (!unavailableDays.Contains(day))
            {
                result.Add(day);
            }
        }

        return result;
    }

    internal static Guid SelectProfessionalDeterministically(
        DateTimeOffset startsAtUtc,
        IEnumerable<AvailableSlot> slots)
    {
        var professionalId = slots
            .Where(slot => slot.Period.StartsAtUtc == startsAtUtc)
            .Select(slot => slot.ProfessionalId)
            .OrderBy(id => id)
            .FirstOrDefault();

        return professionalId == Guid.Empty
            ? throw new DomainRuleException("professional_unavailable", "No eligible professional is available.")
            : professionalId;
    }
}
