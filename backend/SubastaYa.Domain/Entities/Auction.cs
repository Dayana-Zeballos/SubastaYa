namespace SubastaYa.Domain.Entities;

public enum AuctionStatus
{
    /// <summary>Próxima: publicada, pero la ventana de ofertas todavía no abrió.</summary>
    Scheduled = 0,
    Active = 1,

    /// <summary>Finalizada: cerró con ganador y los fondos ya se liquidaron.</summary>
    Finished = 2,

    /// <summary>Desierta: venció sin recibir ninguna puja.</summary>
    Deserted = 3,
    Cancelled = 4
}

public class Auction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public decimal StartingPrice { get; set; }

    /// <summary>Monto mínimo que una nueva puja debe sumar sobre la oferta vigente.</summary>
    public decimal MinimumIncrement { get; set; }

    public decimal CurrentPrice { get; set; }
    public Guid SellerId { get; set; }
    public Guid? HighestBidderId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public AuctionStatus Status { get; set; } = AuctionStatus.Scheduled;

    /// <summary>Optimistic concurrency token (pujas concurrentes → 409).</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Category Category { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public User? HighestBidder { get; set; }
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
