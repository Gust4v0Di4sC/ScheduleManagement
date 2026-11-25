using System.ComponentModel.DataAnnotations;

namespace Reservas.Api.Modules.Platform;

internal sealed class PlatformOptions
{
    internal const string SectionName = "Platform";

    [Required]
    public string CorrelationHeaderName { get; init; } = "X-Correlation-ID";

    [Required]
    public RateLimitOptions RateLimit { get; init; } = new();

    [Required]
    public LocalStorageOptions LocalStorage { get; init; } = new();
}

internal sealed class LocalStorageOptions
{
    [Required]
    public string RootPath { get; init; } = "App_Data/storage";

    [Range(1, 720)]
    public int ExportRetentionHours { get; init; } = 24;

    [Range(1, 1_440)]
    public int ExportCleanupMinutes { get; init; } = 60;
}

internal sealed class RateLimitOptions
{
    [Range(1, 10_000)]
    public int PermitLimit { get; init; } = 100;

    [Range(1, 3_600)]
    public int WindowSeconds { get; init; } = 60;

    [Range(0, 10_000)]
    public int QueueLimit { get; init; }
}

internal sealed class SensitiveConfigurationOptions
{
    internal const string SectionName = "SensitiveConfiguration";

    public bool Enabled { get; init; }

    public string? PlaceholderApiKey { get; init; }
}
