using SubastaYa.Application.Common.Models;

namespace SubastaYa.Application.Features.Auctions;

public interface IAuctionQueryService
{
    Task<PagedResult<AuctionListItemResponse>> SearchAsync(
        AuctionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AuctionDetailResponse> GetByIdAsync(
        Guid id,
        Guid? currentUserId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AuctionListItemResponse>> GetMineAsync(
        Guid sellerId,
        MineQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AuctionListItemResponse>> GetPurchasesAsync(
        Guid bidderId,
        MineQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AuctionListItemResponse>> GetParticipatedAsync(
        Guid bidderId,
        MineQueryParameters parameters,
        CancellationToken cancellationToken = default);
}
