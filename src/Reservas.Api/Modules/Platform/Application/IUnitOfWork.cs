namespace Reservas.Api.Modules.Platform.Application;

internal interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken);
}
