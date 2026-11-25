using Reservas.Api.Modules.Notifications.Application;
using Reservas.Api.Modules.Notifications.Infrastructure;

namespace Reservas.Api.Modules.Notifications;

internal static class NotificationsModule
{
    internal static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ICapturedEmailMailbox, InMemoryCapturedEmailMailbox>();
        return services;
    }

    internal static IEndpointRouteBuilder MapNotificationsModule(this IEndpointRouteBuilder endpoints)
    {
        var environment = endpoints.ServiceProvider.GetRequiredService<IHostEnvironment>();
        if (!environment.IsDevelopment())
        {
            return endpoints;
        }

        endpoints.MapGet("/development/captured-emails", async (
                ICapturedEmailMailbox mailbox,
                CancellationToken cancellationToken) =>
            Results.Ok(await mailbox.GetAllAsync(cancellationToken).ConfigureAwait(false)))
            .ExcludeFromDescription()
            .DisableRateLimiting();

        endpoints.MapDelete("/development/captured-emails", async (
                ICapturedEmailMailbox mailbox,
                CancellationToken cancellationToken) =>
            {
                await mailbox.ClearAsync(cancellationToken).ConfigureAwait(false);
                return Results.NoContent();
            })
            .ExcludeFromDescription()
            .DisableRateLimiting();

        return endpoints;
    }
}
