namespace SubastaYa.Domain.Entities;

public enum AuctionStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2,
    Cancelled = 3
}

public class Auction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public Guid SellerId { get; set; }
    public Guid? HighestBidderId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public AuctionStatus Status { get; set; } = AuctionStatus.Active;

    /// <summary>Optimistic concurrency token (pujas concurrentes → 409).</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public User Seller { get; set; } = null!;
    public User? HighestBidder { get; set; }
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
