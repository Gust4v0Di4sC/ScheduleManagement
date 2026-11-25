using Reservas.Api.Modules.Accounts;
using Reservas.Api.Modules.Catalog;
using Reservas.Api.Modules.Customers;
using Reservas.Api.Modules.Identity;
using Reservas.Api.Modules.Notifications;
using Reservas.Api.Modules.Professionals;
using Reservas.Api.Modules.Scheduling;

namespace Reservas.Api.Modules;

internal static class ModuleRegistration
{
    internal static IServiceCollection AddApplicationModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAccountsModule(configuration);
        services.AddCatalogModule(configuration);
        services.AddCustomersModule(configuration);
        services.AddIdentityModule(configuration);
        services.AddNotificationsModule(configuration);
        services.AddProfessionalsModule(configuration);
        services.AddSchedulingModule(configuration);

        return services;
    }

    internal static IEndpointRouteBuilder MapApplicationModules(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapNotificationsModule();
        return endpoints;
    }
}
