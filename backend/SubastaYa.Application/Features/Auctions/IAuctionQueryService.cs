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
}
