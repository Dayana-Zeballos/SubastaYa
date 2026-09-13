using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Application.Features.Auctions;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly SubastaYaDbContext _context;

    public AuctionRepository(SubastaYaDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<AuctionProjection> Items, int TotalItems)> SearchAsync(
        AuctionQueryParameters parameters,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Auctions.AsNoTracking().AsQueryable();

        query = ApplyStatusFilter(query, parameters.Status, now);

        if (!string.IsNullOrWhiteSpace(parameters.CategorySlug))
        {
            var slug = parameters.CategorySlug.Trim().ToLower();
            query = query.Where(a => a.Category.Slug == slug);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(a => a.CurrentPrice >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(a => a.CurrentPrice <= parameters.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(a => a.Title.Contains(search) || a.Description.Contains(search));
        }

        // Contamos antes de paginar, porque el total es sobre el filtro y no sobre la página.
        var totalItems = await query.CountAsync(cancellationToken);

        query = ApplySort(query, parameters.Sort);

        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(a => new AuctionProjection
            {
                Id = a.Id,
                Title = a.Title,
                ImageUrl = a.ImageUrl,
                CategoryName = a.Category.Name,
                CategorySlug = a.Category.Slug,
                StartingPrice = a.StartingPrice,
                MinimumIncrement = a.MinimumIncrement,
                CurrentPrice = a.CurrentPrice,
                BidCount = a.Bids.Count,
                StartsAt = a.StartsAt,
                EndsAt = a.EndsAt,
                Status = a.Status
            })
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<AuctionDetailProjection?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Auctions
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuctionDetailProjection
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                ImageUrl = a.ImageUrl,
                CategoryName = a.Category.Name,
                CategorySlug = a.Category.Slug,
                StartingPrice = a.StartingPrice,
                MinimumIncrement = a.MinimumIncrement,
                CurrentPrice = a.CurrentPrice,
                BidCount = a.Bids.Count,
                StartsAt = a.StartsAt,
                EndsAt = a.EndsAt,
                Status = a.Status,
                SellerUserName = a.Seller.UserName,
                HighestBidderId = a.HighestBidderId,
                HighestBidderUserName = a.HighestBidder != null ? a.HighestBidder.UserName : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Auction auction, CancellationToken cancellationToken = default)
    {
        _context.Auctions.Add(auction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<Auction?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Auctions.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public void AddBid(Bid bid) => _context.Bids.Add(bid);

    public async Task<IReadOnlyList<Guid>> GetIdsDueForActivationAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await _context.Auctions
            .AsNoTracking()
            .Where(a => a.Status == AuctionStatus.Scheduled && a.StartsAt <= now)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetIdsDueForClosureAsync(
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await _context.Auctions
            .AsNoTracking()
            .Where(a => a.Status == AuctionStatus.Active && a.EndsAt <= now)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);
    }

    // "Finalizadas" en el catálogo incluye las que vencieron y todavía esperan al worker.
    // Si no las contempláramos, desaparecerían de los tres filtros hasta que corra el cierre.
    private static IQueryable<Auction> ApplyStatusFilter(
        IQueryable<Auction> query,
        AuctionStatusFilter? status,
        DateTime now) => status switch
    {
        AuctionStatusFilter.Active => query.Where(a => a.Status == AuctionStatus.Active && a.EndsAt > now),
        AuctionStatusFilter.Scheduled => query.Where(a => a.Status == AuctionStatus.Scheduled),
        AuctionStatusFilter.Finished => query.Where(a =>
            a.Status == AuctionStatus.Finished
            || a.Status == AuctionStatus.Deserted
            || a.Status == AuctionStatus.Cancelled
            || (a.Status == AuctionStatus.Active && a.EndsAt <= now)),
        _ => query
    };

    // El desempate por Id mantiene el orden estable entre páginas: sin él, dos subastas con
    // el mismo precio pueden intercambiarse y aparecer repetidas o salteadas al paginar.
    private static IQueryable<Auction> ApplySort(IQueryable<Auction> query, AuctionSortOrder sort) => sort switch
    {
        AuctionSortOrder.HighestBid => query.OrderByDescending(a => a.CurrentPrice).ThenBy(a => a.Id),
        AuctionSortOrder.LowestPrice => query.OrderBy(a => a.CurrentPrice).ThenBy(a => a.Id),
        _ => query.OrderBy(a => a.EndsAt).ThenBy(a => a.Id)
    };
}
