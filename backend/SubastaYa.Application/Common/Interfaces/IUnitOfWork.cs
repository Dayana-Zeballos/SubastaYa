namespace SubastaYa.Application.Common.Interfaces;

// Agrupa SaveChanges y la transacción explícita del escrow (decisión 10).
public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken = default);
}
