using SubastaYa.Application.Common.Exceptions;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Wallets;

public class WalletService : IWalletService
{
    private const int AmountDecimals = 2;

    private readonly IWalletRepository _wallets;
    private readonly ICurrentUserService _currentUser;

    public WalletService(IWalletRepository wallets, ICurrentUserService currentUser)
    {
        _wallets = wallets;
        _currentUser = currentUser;
    }

    public async Task<WalletBalanceResponse> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        var wallet = await GetWalletAsync(_currentUser.UserId, cancellationToken);
        return ToBalance(wallet);
    }

    public async Task<WalletBalanceResponse> DepositAsync(
        DepositRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new BusinessRuleException("El monto a depositar tiene que ser mayor a cero.");
        }

        if (HasExtraDecimals(request.Amount))
        {
            throw new BusinessRuleException($"Los montos admiten hasta {AmountDecimals} decimales.");
        }

        var wallet = await GetWalletAsync(_currentUser.UserId, cancellationToken);

        wallet.AvailableBalance += request.Amount;

        _wallets.AddTransaction(new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = WalletTransactionType.Deposit,
            Amount = request.Amount,
            Description = "Depósito de fondos",
            CreatedAt = DateTime.UtcNow
        });

        // Un solo SaveChanges: el RowVersion de Wallet cubre dos depósitos concurrentes (409).
        await _wallets.SaveChangesAsync(cancellationToken);

        return ToBalance(wallet);
    }

    public async Task ReserveAsync(
        Guid userId,
        decimal amount,
        Guid? auctionId,
        string description,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("El monto a retener tiene que ser mayor a cero.");
        }

        var wallet = await GetWalletAsync(userId, cancellationToken);

        if (wallet.AvailableBalance < amount)
        {
            throw new BusinessRuleException("Saldo disponible insuficiente para cubrir la oferta.");
        }

        wallet.AvailableBalance -= amount;
        wallet.ReservedBalance += amount;

        _wallets.AddTransaction(new WalletTransaction
        {
            WalletId = wallet.Id,
            AuctionId = auctionId,
            Type = WalletTransactionType.Reserve,
            Amount = amount,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task ReleaseAsync(
        Guid userId,
        decimal amount,
        Guid? auctionId,
        string description,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("El monto a liberar tiene que ser mayor a cero.");
        }

        var wallet = await GetWalletAsync(userId, cancellationToken);

        if (wallet.ReservedBalance < amount)
        {
            throw new BusinessRuleException("No hay fondos retenidos suficientes para liberar.");
        }

        wallet.ReservedBalance -= amount;
        wallet.AvailableBalance += amount;

        _wallets.AddTransaction(new WalletTransaction
        {
            WalletId = wallet.Id,
            AuctionId = auctionId,
            Type = WalletTransactionType.Release,
            Amount = amount,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task<Wallet> GetWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var wallet = await _wallets.GetByUserIdAsync(userId, cancellationToken);

        if (wallet is null)
        {
            throw NotFoundException.For("billetera del usuario", userId);
        }

        return wallet;
    }

    private static WalletBalanceResponse ToBalance(Wallet wallet) => new()
    {
        UserId = wallet.UserId,
        Available = wallet.AvailableBalance,
        Reserved = wallet.ReservedBalance,
        Total = wallet.AvailableBalance + wallet.ReservedBalance
    };

    private static bool HasExtraDecimals(decimal amount) =>
        decimal.Round(amount, AmountDecimals) != amount;
}
