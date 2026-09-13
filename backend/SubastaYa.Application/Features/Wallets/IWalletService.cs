namespace SubastaYa.Application.Features.Wallets;

public interface IWalletService
{
    Task<WalletBalanceResponse> GetBalanceAsync(CancellationToken cancellationToken = default);

    Task<WalletBalanceResponse> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default);

    // Mueven saldo y escriben el ledger, pero no hacen SaveChanges: el caller (la puja)
    // las mete dentro de su propia transacción explícita.
    Task ReserveAsync(
        Guid userId,
        decimal amount,
        Guid? auctionId,
        string description,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(
        Guid userId,
        decimal amount,
        Guid? auctionId,
        string description,
        CancellationToken cancellationToken = default);
}
