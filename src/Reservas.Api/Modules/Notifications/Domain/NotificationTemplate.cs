using System.Text.RegularExpressions;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Notifications.Domain;

internal sealed partial record NotificationTemplate(
    string Key,
    string SubjectTemplate,
    string BodyTemplate)
{
    internal string RenderSubject(IReadOnlyDictionary<string, string> values) => Render(SubjectTemplate, values);

    internal string RenderBody(IReadOnlyDictionary<string, string> values) => Render(BodyTemplate, values);

    private static string Render(string template, IReadOnlyDictionary<string, string> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(values);
        var requiredKeys = Placeholder().Matches(template).Select(match => match.Groups[1].Value).Distinct().ToArray();
        var missing = requiredKeys.Where(key => !values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value)).ToArray();
        if (missing.Length > 0)
        {
            throw new DomainRuleException(
                "notification_template_data_missing",
                $"Template data is missing: {string.Join(", ", missing)}.");
        }

        return Placeholder().Replace(template, match => values[match.Groups[1].Value]);
    }

    [GeneratedRegex("\\{\\{([a-zA-Z0-9_.-]+)\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex Placeholder();
}
