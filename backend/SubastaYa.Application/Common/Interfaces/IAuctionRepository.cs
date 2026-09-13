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

    // Con tracking: el RowVersion tiene que viajar en el UPDATE para poder devolver 409.
    Task<Auction?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void AddBid(Bid bid);

    Task<IReadOnlyList<Guid>> GetIdsDueForActivationAsync(
        DateTime now,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetIdsDueForClosureAsync(
        DateTime now,
        CancellationToken cancellationToken = default);
}
