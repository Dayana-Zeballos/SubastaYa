using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Common.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void AddTransaction(WalletTransaction transaction);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WalletTransaction>> GetTransactionsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
