using SubastaYa.Application.Common.Exceptions;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Features.Audit;
using SubastaYa.Application.Features.Wallets;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Bidding;

public class BiddingService : IBiddingService
{
    // Decisión 11: umbrales con nombre, no números sueltos en el if.
    private static readonly TimeSpan AntiSnipingWindow = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan AntiSnipingExtension = TimeSpan.FromMinutes(2);
    private const int AmountDecimals = 2;

    private readonly IAuctionRepository _auctions;
    private readonly IWalletService _wallets;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    public BiddingService(
        IAuctionRepository auctions,
        IWalletService wallets,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IAuditService audit)
    {
        _auctions = auctions;
        _wallets = wallets;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<BidResponse> PlaceBidAsync(
        Guid auctionId,
        PlaceBidRequest request,
        CancellationToken cancellationToken = default)
    {
        var bidderId = _currentUser.UserId;
        BidResponse? response = null;

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var now = DateTime.UtcNow;
                var auction = await _auctions.GetTrackedByIdAsync(auctionId, ct)
                    ?? throw NotFoundException.For("subasta", auctionId);

                ValidateAuctionIsOpen(auction, now);
                ValidateBidder(auction, bidderId);
                ValidateAmount(auction, request.Amount);

                var previousBidderId = auction.HighestBidderId;
                var previousAmount = auction.CurrentPrice;

                // Si alguien ya lideraba, liberamos su retención antes de congelar la nueva.
                // Si el mismo postor mejora su oferta, también liberamos la anterior y retenemos
                // el monto completo nuevo (más simple y deja el ledger consistente).
                if (previousBidderId.HasValue)
                {
                    await _wallets.ReleaseAsync(
                        previousBidderId.Value,
                        previousAmount,
                        auction.Id,
                        "Liberación al ser superado por otra puja",
                        ct);
                }

                await _wallets.ReserveAsync(
                    bidderId,
                    request.Amount,
                    auction.Id,
                    "Retención por puja líder",
                    ct);

                var bid = new Bid
                {
                    AuctionId = auction.Id,
                    BidderId = bidderId,
                    Amount = request.Amount,
                    CreatedAt = now
                };

                _auctions.AddBid(bid);

                auction.CurrentPrice = request.Amount;
                auction.HighestBidderId = bidderId;

                var antiSnipingApplied = false;
                if (auction.EndsAt - now <= AntiSnipingWindow)
                {
                    auction.EndsAt = auction.EndsAt.Add(AntiSnipingExtension);
                    antiSnipingApplied = true;
                }

                response = new BidResponse
                {
                    Id = bid.Id,
                    AuctionId = auction.Id,
                    Amount = bid.Amount,
                    CreatedAt = bid.CreatedAt,
                    AuctionEndsAt = auction.EndsAt,
                    AntiSnipingApplied = antiSnipingApplied
                };
            }, cancellationToken);
        }
        catch (BusinessRuleException ex)
        {
            await _audit.LogAsync(
                nameof(Auction),
                auctionId.ToString(),
                AuditActions.BidRejected,
                ex.Message,
                bidderId,
                cancellationToken);
            throw;
        }
        catch (ConflictException ex)
        {
            await _audit.LogAsync(
                nameof(Auction),
                auctionId.ToString(),
                AuditActions.BidRejected,
                $"Concurrencia: {ex.Message}",
                bidderId,
                cancellationToken);
            throw;
        }

        await _audit.LogAsync(
            nameof(Auction),
            auctionId.ToString(),
            AuditActions.BidPlaced,
            $"Monto={response!.Amount:0.00}",
            bidderId,
            cancellationToken);

        if (response.AntiSnipingApplied)
        {
            await _audit.LogAsync(
                nameof(Auction),
                auctionId.ToString(),
                AuditActions.AuctionExtended,
                $"NuevaEndsAt={response.AuctionEndsAt:o}",
                bidderId,
                cancellationToken);
        }

        return response;
    }

    private static void ValidateAuctionIsOpen(Auction auction, DateTime now)
    {
        if (auction.Status != AuctionStatus.Active)
        {
            throw new BusinessRuleException("Solo se puede pujar en subastas activas.");
        }

        if (now < auction.StartsAt)
        {
            throw new BusinessRuleException("La subasta todavía no abrió para ofertas.");
        }

        if (now >= auction.EndsAt)
        {
            throw new BusinessRuleException("La subasta ya cerró.");
        }
    }

    private static void ValidateBidder(Auction auction, Guid bidderId)
    {
        if (auction.SellerId == bidderId)
        {
            throw new BusinessRuleException("El vendedor no puede pujar en su propia subasta.");
        }
    }

    private static void ValidateAmount(Auction auction, decimal amount)
    {
        if (amount <= 0)
        {
            throw new BusinessRuleException("El monto de la puja tiene que ser mayor a cero.");
        }

        if (decimal.Round(amount, AmountDecimals) != amount)
        {
            throw new BusinessRuleException($"Los montos admiten hasta {AmountDecimals} decimales.");
        }

        // Con ofertas previas hay que superar precio + incremento. Sin ofertas alcanza
        // cubrir el precio base (CurrentPrice arranca igual al StartingPrice).
        var minimum = auction.HighestBidderId.HasValue
            ? auction.CurrentPrice + auction.MinimumIncrement
            : auction.CurrentPrice;

        if (amount < minimum)
        {
            throw new BusinessRuleException(
                $"La oferta mínima admitida es {minimum:0.00}.");
        }
    }
}
