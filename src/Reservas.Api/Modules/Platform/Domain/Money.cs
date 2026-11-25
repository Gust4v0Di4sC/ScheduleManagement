namespace Reservas.Api.Modules.Platform.Domain;

internal readonly record struct Money
{
    internal Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainRuleException("money_negative", "A monetary amount cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3 || !normalizedCurrency.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(currency));
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = normalizedCurrency;
    }

    internal decimal Amount { get; }

    internal string Currency { get; }

    public static Money operator +(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
        {
            throw new DomainRuleException("currency_mismatch", "Money with different currencies cannot be added.");
        }

        return new Money(left.Amount + right.Amount, left.Currency);
    }
}
