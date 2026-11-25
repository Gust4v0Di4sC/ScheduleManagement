using System.Net.Mail;

namespace Reservas.Api.Modules.Platform.Domain;

internal readonly record struct EmailAddress
{
    private EmailAddress(string value) => Value = value;

    internal string Value { get; }

    internal static EmailAddress Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 254 || !MailAddress.TryCreate(normalized, out var address) || address.Address != normalized)
        {
            throw new DomainRuleException("email_invalid", "The email address is invalid.");
        }

        return new EmailAddress(normalized);
    }

    public override string ToString() => Value;
}
