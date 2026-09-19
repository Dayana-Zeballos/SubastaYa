using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly SubastaYaDbContext _context;

    public WalletRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

    public void AddTransaction(WalletTransaction transaction) =>
        _context.WalletTransactions.Add(transaction);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<WalletTransaction>> GetTransactionsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WalletTransactions
            .AsNoTracking()
            .Where(t => t.Wallet.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);
    }
}
