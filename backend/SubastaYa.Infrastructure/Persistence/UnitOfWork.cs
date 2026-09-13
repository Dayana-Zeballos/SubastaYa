using SubastaYa.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;

namespace SubastaYa.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly SubastaYaDbContext _context;

    public UnitOfWork(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> work,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await work(cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            // La capa Application no referencia EF: traducimos acá a ConflictException (409).
            throw new ConflictException(
                "Otra operación modificó el recurso mientras procesábamos la tuya. Volvé a intentarlo.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            throw;
        }
    }
}
