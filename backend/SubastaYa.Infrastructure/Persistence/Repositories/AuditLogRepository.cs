using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly SubastaYaDbContext _context;

    public AuditLogRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public void Add(AuditLog entry) => _context.AuditLogs.Add(entry);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
