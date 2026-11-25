using Reservas.Api.Modules.Identity.Application;
using Reservas.Api.Modules.Identity.Infrastructure;

namespace Reservas.Api.Modules.Identity;

internal static class IdentityModule
{
    internal static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IIdentityEmailSender, CapturedIdentityEmailSender>();
        return services;
    }
}
