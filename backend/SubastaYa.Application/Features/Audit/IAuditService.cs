namespace SubastaYa.Application.Features.Audit;

public interface IAuditService
{
    Task LogAsync(
        string entityName,
        string entityId,
        string action,
        string? details = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}
