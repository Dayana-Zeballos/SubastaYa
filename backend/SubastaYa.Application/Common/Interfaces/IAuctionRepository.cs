using SubastaYa.Application.Features.Auctions;
using SubastaYa.Application.Features.Bidding;
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

    // Con tracking: el RowVersion tiene que viajar en el UPDATE para poder devolver 409.
    Task<Auction?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void AddBid(Bid bid);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BidHistoryProjection>> GetBidsAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetIdsDueForActivationAsync(
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetIdsDueForClosureAsync(
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AuctionProjection> Items, int TotalItems)> ListBySellerAsync(
        Guid sellerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AuctionProjection> Items, int TotalItems)> ListWonByBidderAsync(
        Guid bidderId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AuctionProjection> Items, int TotalItems)> ListParticipatedAsync(
        Guid bidderId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
