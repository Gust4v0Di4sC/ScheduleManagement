using System.Text.RegularExpressions;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Accounts.Domain;

internal readonly partial record struct Subdomain
{
    private Subdomain(string value) => Value = value;

    internal string Value { get; }

    internal static Subdomain Create(string value, IReadOnlySet<string> reservedNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentNullException.ThrowIfNull(reservedNames);

        var normalized = value.Trim().ToLowerInvariant();
        if (!ValidSubdomain().IsMatch(normalized))
        {
            throw new DomainRuleException(
                "subdomain_invalid",
                "A subdomain must contain 3 to 63 lowercase letters, numbers, or interior hyphens.");
        }

        if (reservedNames.Contains(normalized))
        {
            throw new DomainRuleException("subdomain_reserved", "The requested subdomain is reserved.");
        }

        return new Subdomain(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidSubdomain();
}
