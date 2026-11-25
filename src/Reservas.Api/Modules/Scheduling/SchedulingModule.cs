using Reservas.Api.Modules.Scheduling.Application;
using Reservas.Api.Modules.Scheduling.Infrastructure;

namespace Reservas.Api.Modules.Scheduling;

internal static class SchedulingModule
{
    internal static IServiceCollection AddSchedulingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IPaymentGateway, SimulatedPaymentGateway>();
        return services;
    }
}
