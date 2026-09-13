using SubastaYa.Application.Common.Exceptions;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Features.Audit;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Wallets;

public class WalletService : IWalletService
{
    private const int AmountDecimals = 2;

    private readonly IWalletRepository _wallets;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public WalletService(
        IWalletRepository wallets,
        ICurrentUserService currentUser,
        IAuditService audit)
    {
        _wallets = wallets;
        _currentUser = currentUser;
        _audit = audit;
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

        var userId = _currentUser.UserId;
        var wallet = await GetWalletAsync(userId, cancellationToken);

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

        await _audit.LogAsync(
            nameof(Wallet),
            wallet.Id.ToString(),
            AuditActions.WalletDeposit,
            $"Monto={request.Amount:0.00}",
            userId,
            cancellationToken);

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

    public async Task CaptureAsync(
        Guid userId,
        decimal amount,
        Guid? auctionId,
        string description,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("El monto a cobrar tiene que ser mayor a cero.");
        }

        var wallet = await GetWalletAsync(userId, cancellationToken);

        if (wallet.ReservedBalance < amount)
        {
            throw new BusinessRuleException("Fondos retenidos insuficientes para liquidar la venta.");
        }

        wallet.ReservedBalance -= amount;

        _wallets.AddTransaction(new WalletTransaction
        {
            WalletId = wallet.Id,
            AuctionId = auctionId,
            Type = WalletTransactionType.Capture,
            Amount = amount,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task CreditAsync(
        Guid userId,
        decimal amount,
        Guid? auctionId,
        string description,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("El monto a acreditar tiene que ser mayor a cero.");
        }

        var wallet = await GetWalletAsync(userId, cancellationToken);

        wallet.AvailableBalance += amount;

        _wallets.AddTransaction(new WalletTransaction
        {
            WalletId = wallet.Id,
            AuctionId = auctionId,
            Type = WalletTransactionType.Deposit,
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
