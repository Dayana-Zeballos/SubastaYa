using SubastaYa.Application.Common.Exceptions;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Features.Audit;
using SubastaYa.Application.Features.Wallets;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Auctions;

// Lógica de negocio del cierre/activación. El BackgroundService solo orquesta el ciclo.
public class AuctionClosureService : IAuctionClosureService
{
    private readonly IAuctionRepository _auctions;
    private readonly IWalletService _wallets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    public AuctionClosureService(
        IAuctionRepository auctions,
        IWalletService wallets,
        IUnitOfWork unitOfWork,
        IAuditService audit)
    {
        _auctions = auctions;
        _wallets = wallets;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task ProcessDueAuctionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var toActivate = await _auctions.GetIdsDueForActivationAsync(now, cancellationToken);
        foreach (var auctionId in toActivate)
        {
            await TryProcessOneAsync(auctionId, ActivateAsync, cancellationToken);
        }

        var toClose = await _auctions.GetIdsDueForClosureAsync(now, cancellationToken);
        foreach (var auctionId in toClose)
        {
            await TryProcessOneAsync(auctionId, CloseAsync, cancellationToken);
        }
    }

    private async Task TryProcessOneAsync(
        Guid auctionId,
        Func<Guid, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        try
        {
            await action(auctionId, cancellationToken);
        }
        catch (ConflictException)
        {
            // Carrera con una puja de último segundo: el próximo ciclo lo reintenta.
        }
    }

    private async Task ActivateAsync(Guid auctionId, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var now = DateTime.UtcNow;
            var auction = await _auctions.GetTrackedByIdAsync(auctionId, ct);
            if (auction is null
                || auction.Status != AuctionStatus.Scheduled
                || auction.StartsAt > now)
            {
                return;
            }

            auction.Status = AuctionStatus.Active;
        }, cancellationToken);

        await _audit.LogAsync(
            nameof(Auction),
            auctionId.ToString(),
            AuditActions.AuctionActivated,
            "Activada por el worker al llegar StartsAt",
            userId: null,
            cancellationToken);
    }

    private async Task CloseAsync(Guid auctionId, CancellationToken cancellationToken)
    {
        string? action = null;
        string? details = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var now = DateTime.UtcNow;
            var auction = await _auctions.GetTrackedByIdAsync(auctionId, ct);
            if (auction is null
                || auction.Status != AuctionStatus.Active
                || auction.EndsAt > now)
            {
                return;
            }

            if (auction.HighestBidderId is Guid winnerId)
            {
                var amount = auction.CurrentPrice;

                await _wallets.CaptureAsync(
                    winnerId,
                    amount,
                    auction.Id,
                    "Cobro de subasta ganada",
                    ct);

                await _wallets.CreditAsync(
                    auction.SellerId,
                    amount,
                    auction.Id,
                    "Acreditación por venta",
                    ct);

                auction.Status = AuctionStatus.Finished;
                action = AuditActions.AuctionFinished;
                details = $"Ganador={winnerId}; Monto={amount:0.00}";
            }
            else
            {
                auction.Status = AuctionStatus.Deserted;
                action = AuditActions.AuctionDeserted;
                details = "Cerrada sin ofertas";
            }
        }, cancellationToken);

        if (action is not null)
        {
            await _audit.LogAsync(
                nameof(Auction),
                auctionId.ToString(),
                action,
                details,
                userId: null,
                cancellationToken);
        }
    }
}
