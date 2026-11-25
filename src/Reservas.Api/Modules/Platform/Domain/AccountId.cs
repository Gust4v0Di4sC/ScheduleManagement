namespace Reservas.Api.Modules.Platform.Domain;

internal readonly record struct AccountId
{
    internal AccountId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Account identifier cannot be empty.", nameof(value));
        }

        Value = value;
    }

    internal Guid Value { get; }

    public override string ToString() => Value.ToString();
}
