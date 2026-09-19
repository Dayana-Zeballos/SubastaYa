namespace SubastaYa.Application.Features.Bidding;

public interface IBiddingService
{
    Task<BidResponse> PlaceBidAsync(
        Guid auctionId,
        PlaceBidRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BidHistoryItemResponse>> GetBidsAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);
}
