namespace SubastaYa.Application.Features.Auctions;

public interface IAuctionCommandService
{
    Task<AuctionDetailResponse> CreateAsync(
        CreateAuctionRequest request,
        CancellationToken cancellationToken = default);
}
