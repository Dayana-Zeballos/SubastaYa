namespace SubastaYa.Application.Features.Auctions;

public interface IAuctionClosureService
{
    Task ProcessDueAuctionsAsync(CancellationToken cancellationToken = default);
}
