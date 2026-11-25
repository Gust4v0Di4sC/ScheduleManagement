using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Reservas.Api.Modules.Platform.Application;
using Reservas.Api.Modules.Platform.Infrastructure;

namespace Reservas.Api.Modules.Platform;

internal static class PlatformModule
{
    internal static IServiceCollection AddPlatformModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<PlatformOptions>()
            .Bind(configuration.GetSection(PlatformOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => !string.IsNullOrWhiteSpace(options.CorrelationHeaderName),
                "Platform:CorrelationHeaderName must not be empty.")
            .Validate(options => options.RateLimit.PermitLimit > 0, "Rate limit must be positive.")
            .Validate(options => options.RateLimit.WindowSeconds > 0, "Rate-limit window must be positive.")
            .ValidateOnStart();

        services.AddControllers();
        services.AddOpenApi();
        services.AddHealthChecks();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
            };
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IExportArtifactStore, LocalExportArtifactStore>();
        services.AddHostedService<ExportCleanupService>();
        services.AddScoped<CurrentAccountContext>();
        services.AddScoped<IAccountContext>(provider => provider.GetRequiredService<CurrentAccountContext>());
        services.AddAuthorization();
        services.AddRateLimiter(options =>
        {
            var settings = configuration.GetSection(PlatformOptions.SectionName).Get<PlatformOptions>()
                ?? new PlatformOptions();

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(PlatformPolicies.ApiRateLimit, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = settings.RateLimit.PermitLimit,
                        QueueLimit = settings.RateLimit.QueueLimit,
                        Window = TimeSpan.FromSeconds(settings.RateLimit.WindowSeconds)
                    }));
        });

        return services;
    }
}
