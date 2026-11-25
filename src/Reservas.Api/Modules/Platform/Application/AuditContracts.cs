using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Platform.Application;

internal interface IAuditStore
{
    Task AppendAsync(AuditEntry entry, CancellationToken cancellationToken);
}

internal sealed class AuditService(IAuditStore store)
{
    internal Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return store.AppendAsync(entry, cancellationToken);
    }
}
