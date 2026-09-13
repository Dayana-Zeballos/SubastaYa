using SubastaYa.Application.Features.Auctions;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Common.Interfaces;

public interface IAuctionRepository
{
    Task<(IReadOnlyList<AuctionProjection> Items, int TotalItems)> SearchAsync(
        AuctionQueryParameters parameters,
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<AuctionDetailProjection?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Auction auction, CancellationToken cancellationToken = default);
}
