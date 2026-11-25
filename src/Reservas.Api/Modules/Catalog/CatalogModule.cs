using Reservas.Api.Modules.Catalog.Application;
using Reservas.Api.Modules.Catalog.Infrastructure;

namespace Reservas.Api.Modules.Catalog;

internal static class CatalogModule
{
    internal static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IObjectStorage, LocalObjectStorage>();
        return services;
    }
}
