using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Features.Auctions;

// Lo que el repositorio trae de la base, con los campos crudos. El servicio se encarga de
// derivar el estado efectivo y la próxima puja sugerida antes de devolverlo al cliente.
public class AuctionProjection
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal MinimumIncrement { get; set; }
    public decimal CurrentPrice { get; set; }
    public int BidCount { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public AuctionStatus Status { get; set; }
}

public class AuctionDetailProjection : AuctionProjection
{
    public string Description { get; set; } = string.Empty;
    public string SellerUserName { get; set; } = string.Empty;
    public string? HighestBidderUserName { get; set; }
    public Guid? HighestBidderId { get; set; }
}
