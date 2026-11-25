using Microsoft.Extensions.Options;

namespace Reservas.Api.Modules.Platform;

internal sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    IOptions<PlatformOptions> options,
    ILogger<CorrelationIdMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headerName = options.Value.CorrelationHeaderName;
        var requestedId = context.Request.Headers[headerName].FirstOrDefault();
        var correlationId = IsValid(requestedId) ? requestedId! : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[headerName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value.All(character => !char.IsControl(character));
}
