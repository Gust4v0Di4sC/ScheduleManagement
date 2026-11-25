using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Scheduling.Domain;

internal sealed record QuoteLine(string Code, string Description, Money Amount);

internal sealed record QuoteSnapshot(
    Guid PolicyId,
    IReadOnlyCollection<QuoteLine> Lines,
    Money Total,
    TimeSpan Duration,
    DateTimeOffset QuotedAt)
{
    internal static QuoteSnapshot Create(
        Guid policyId,
        IEnumerable<QuoteLine> lines,
        TimeSpan duration,
        DateTimeOffset quotedAt)
    {
        if (policyId == Guid.Empty || duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Quote policy and duration are required.");
        }

        var materializedLines = lines.ToArray();
        if (materializedLines.Length == 0)
        {
            throw new DomainRuleException("quote_empty", "A quote must contain at least one line.");
        }

        var total = materializedLines.Select(line => line.Amount).Aggregate((left, right) => left + right);
        return new QuoteSnapshot(policyId, materializedLines, total, duration, quotedAt);
    }
}

internal static class QuoteCalculator
{
    internal static QuoteSnapshot Calculate(
        Guid policyId,
        Money basePrice,
        Money? professionalPrice,
        Money? locationFee,
        TimeSpan duration,
        DateTimeOffset quotedAt)
    {
        var lines = new List<QuoteLine>
        {
            new("service", "Service", professionalPrice ?? basePrice)
        };
        if (locationFee.HasValue)
        {
            lines.Add(new QuoteLine("location_fee", "Customer location fee", locationFee.Value));
        }

        return QuoteSnapshot.Create(policyId, lines, duration, quotedAt);
    }
}
