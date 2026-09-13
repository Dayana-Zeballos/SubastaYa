using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Audit;

// Append-only: solo inserta. Cada LogAsync persiste de inmediato para que un rechazo
// de puja (después de un rollback) igual deje rastro.
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogs;

    public AuditService(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task LogAsync(
        string entityName,
        string entityId,
        string action,
        string? details = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        _auditLogs.Add(new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            Details = details,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _auditLogs.SaveChangesAsync(cancellationToken);
    }
}
