namespace Reservas.Api.Modules.Platform.Domain;

internal sealed class DomainRuleException(string code, string message) : InvalidOperationException(message)
{
    internal string Code { get; } = code;
}
