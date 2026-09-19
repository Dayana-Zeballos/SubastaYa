using SubastaYa.Application.Common;
using SubastaYa.Application.Common.Exceptions;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Common.Models;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Auctions;

public class AuctionQueryService : IAuctionQueryService
{
    private readonly IAuctionRepository _auctions;

    public AuctionQueryService(IAuctionRepository auctions)
    {
        _auctions = auctions;
    }

    public async Task<PagedResult<AuctionListItemResponse>> SearchAsync(
        AuctionQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        Validate(parameters);

        var now = DateTime.UtcNow;
        var (items, totalItems) = await _auctions.SearchAsync(parameters, now, cancellationToken);

        return new PagedResult<AuctionListItemResponse>
        {
            Items = items.Select(a => ToListItem(a, now)).ToList(),
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public Task<PagedResult<AuctionListItemResponse>> GetMineAsync(
        Guid sellerId,
        MineQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        PagePersonalAsync(parameters, (page, pageSize, ct) =>
            _auctions.ListBySellerAsync(sellerId, page, pageSize, ct), cancellationToken);

    public Task<PagedResult<AuctionListItemResponse>> GetPurchasesAsync(
        Guid bidderId,
        MineQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        PagePersonalAsync(parameters, (page, pageSize, ct) =>
            _auctions.ListWonByBidderAsync(bidderId, page, pageSize, ct), cancellationToken);

    public Task<PagedResult<AuctionListItemResponse>> GetParticipatedAsync(
        Guid bidderId,
        MineQueryParameters parameters,
        CancellationToken cancellationToken = default) =>
        PagePersonalAsync(parameters, (page, pageSize, ct) =>
            _auctions.ListParticipatedAsync(bidderId, page, pageSize, ct), cancellationToken);

    private async Task<PagedResult<AuctionListItemResponse>> PagePersonalAsync(
        MineQueryParameters parameters,
        Func<int, int, CancellationToken, Task<(IReadOnlyList<AuctionProjection> Items, int TotalItems)>> load,
        CancellationToken cancellationToken)
    {
        ValidatePaging(parameters.Page, parameters.PageSize);

        var now = DateTime.UtcNow;
        var (items, totalItems) = await load(parameters.Page, parameters.PageSize, cancellationToken);

        return new PagedResult<AuctionListItemResponse>
        {
            Items = items.Select(a => ToListItem(a, now)).ToList(),
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AuctionDetailResponse> GetByIdAsync(
        Guid id,
        Guid? currentUserId,
        CancellationToken cancellationToken = default)
    {
        var auction = await _auctions.GetDetailAsync(id, cancellationToken)
            ?? throw NotFoundException.For("la subasta", id);

        var now = DateTime.UtcNow;

        return new AuctionDetailResponse
        {
            Id = auction.Id,
            Title = auction.Title,
            Description = auction.Description,
            ImageUrl = auction.ImageUrl,
            CategoryName = auction.CategoryName,
            CategorySlug = auction.CategorySlug,
            StartingPrice = auction.StartingPrice,
            MinimumIncrement = auction.MinimumIncrement,
            CurrentPrice = auction.CurrentPrice,
            NextMinimumBid = CalculateNextMinimumBid(auction),
            BidCount = auction.BidCount,
            SellerUserName = auction.SellerUserName,
            HighestBidderAlias = BidderAlias.FromUserName(auction.HighestBidderUserName),
            StartsAt = auction.StartsAt,
            EndsAt = auction.EndsAt,
            Status = ResolveStatus(auction, now),
            SecondsRemaining = CalculateSecondsRemaining(auction.EndsAt, now),
            IsCurrentUserWinning = currentUserId is null
                ? null
                : auction.HighestBidderId == currentUserId
        };
    }

    private static void Validate(AuctionQueryParameters parameters)
    {
        ValidatePaging(parameters.Page, parameters.PageSize);

        if (parameters.MinPrice < 0 || parameters.MaxPrice < 0)
        {
            throw new BusinessRuleException("Los precios del filtro no pueden ser negativos.");
        }

        if (parameters.MinPrice.HasValue && parameters.MaxPrice.HasValue
            && parameters.MinPrice > parameters.MaxPrice)
        {
            throw new BusinessRuleException("El precio mínimo no puede ser mayor que el máximo.");
        }
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new BusinessRuleException("El número de página tiene que ser 1 o mayor.");
        }

        if (pageSize < 1 || pageSize > AuctionQueryParameters.MaxPageSize)
        {
            throw new BusinessRuleException(
                $"El tamaño de página tiene que estar entre 1 y {AuctionQueryParameters.MaxPageSize}.");
        }
    }

    private static AuctionListItemResponse ToListItem(AuctionProjection auction, DateTime now) => new()
    {
        Id = auction.Id,
        Title = auction.Title,
        ImageUrl = auction.ImageUrl,
        CategoryName = auction.CategoryName,
        CategorySlug = auction.CategorySlug,
        CurrentPrice = auction.CurrentPrice,
        BidCount = auction.BidCount,
        StartsAt = auction.StartsAt,
        EndsAt = auction.EndsAt,
        Status = ResolveStatus(auction, now),
        SecondsRemaining = CalculateSecondsRemaining(auction.EndsAt, now)
    };

    // Una subasta que venció pero todavía no pasó por el worker sigue guardada como Active.
    // Mostrarla como activa sería mentir, así que la reportamos como "closing": cerró y está
    // esperando la liquidación. Así el catálogo es correcto sin importar cuánto tarde el worker.
    private static string ResolveStatus(AuctionProjection auction, DateTime now) => auction.Status switch
    {
        AuctionStatus.Active when auction.EndsAt <= now => "closing",
        AuctionStatus.Active => "active",
        AuctionStatus.Scheduled => "scheduled",
        AuctionStatus.Finished => "finished",
        AuctionStatus.Deserted => "deserted",
        AuctionStatus.Cancelled => "cancelled",
        _ => "unknown"
    };

    private static long CalculateSecondsRemaining(DateTime endsAt, DateTime now)
    {
        var remaining = (long)(endsAt - now).TotalSeconds;

        return remaining > 0 ? remaining : 0;
    }

    private static decimal CalculateNextMinimumBid(AuctionProjection auction) =>
        auction.BidCount == 0
            ? auction.StartingPrice
            : auction.CurrentPrice + auction.MinimumIncrement;
}
