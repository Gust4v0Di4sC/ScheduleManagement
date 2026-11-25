using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Customers.Domain;

internal sealed record ConsentRecord(
    string Version,
    string Origin,
    DateTimeOffset AcceptedAt);

internal readonly record struct BrazilianTaxId
{
    private BrazilianTaxId(string value) => Value = value;

    internal string Value { get; }

    internal static BrazilianTaxId Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (!IsValidCpf(digits))
        {
            throw new DomainRuleException("cpf_invalid", "The CPF is invalid.");
        }

        return new BrazilianTaxId(digits);
    }

    private static bool IsValidCpf(string value)
    {
        if (value.Length != 11 || value.All(character => character == value[0]))
        {
            return false;
        }

        var first = CalculateDigit(value.AsSpan(0, 9), 10);
        var second = CalculateDigit(value.AsSpan(0, 10), 11);
        return value[9] - '0' == first && value[10] - '0' == second;
    }

    private static int CalculateDigit(ReadOnlySpan<char> digits, int weight)
    {
        var sum = 0;
        foreach (var digit in digits)
        {
            sum += (digit - '0') * weight--;
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}

internal sealed class Customer
{
    private readonly List<ConsentRecord> _consents = [];

    internal Customer(
        Guid id,
        AccountId accountId,
        string name,
        EmailAddress email,
        BrazilianTaxId? taxId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id == Guid.Empty ? throw new ArgumentException("Customer identifier is required.", nameof(id)) : id;
        AccountId = accountId;
        Name = name.Trim();
        Email = email;
        TaxId = taxId;
    }

    internal Guid Id { get; }

    internal AccountId AccountId { get; }

    internal string Name { get; }

    internal EmailAddress Email { get; }

    internal BrazilianTaxId? TaxId { get; }

    internal IReadOnlyCollection<ConsentRecord> Consents => _consents.AsReadOnly();

    internal void RecordConsent(string version, string origin, DateTimeOffset acceptedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        if (_consents.Any(consent => consent.Version == version))
        {
            return;
        }

        _consents.Add(new ConsentRecord(version.Trim(), origin.Trim(), acceptedAt));
    }
}
