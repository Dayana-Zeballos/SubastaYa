namespace SubastaYa.Application.Features.Wallets;

public interface IWalletService
{
    Task<WalletBalanceResponse> GetBalanceAsync(CancellationToken cancellationToken = default);

    Task<WalletBalanceResponse> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default);
}
