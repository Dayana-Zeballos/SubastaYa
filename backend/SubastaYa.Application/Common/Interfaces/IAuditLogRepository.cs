using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    void Add(AuditLog entry);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
