using Microsoft.Extensions.Options;
using Reservas.Api.Modules.Platform.Application;

namespace Reservas.Api.Modules.Platform.Infrastructure;

internal sealed class ExportCleanupService(
    IExportArtifactStore artifactStore,
    IClock clock,
    IOptions<PlatformOptions> options,
    ILogger<ExportCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(options.Value.LocalStorage.ExportCleanupMinutes);
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                var deleted = await artifactStore.DeleteExpiredAsync(clock.UtcNow, stoppingToken).ConfigureAwait(false);
                if (deleted > 0)
                {
                    logger.LogInformation("Deleted {ExportCount} expired account exports.", deleted);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Expired account exports could not be cleaned up.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
    }
}
