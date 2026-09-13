namespace SubastaYa.Domain.Entities;

public enum AuctionStatus
{
    // Próxima: publicada, pero la ventana de ofertas todavía no abrió.
    Scheduled = 0,
    Active = 1,
    // Finalizada: cerró con ganador y los fondos ya se liquidaron.
    Finished = 2,
    // Desierta: venció sin recibir ninguna puja.
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
    // Monto mínimo que una nueva puja debe sumar sobre la oferta vigente.
    public decimal MinimumIncrement { get; set; }
    public decimal CurrentPrice { get; set; }
    public Guid SellerId { get; set; }
    public Guid? HighestBidderId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public AuctionStatus Status { get; set; } = AuctionStatus.Scheduled;

    // Token de concurrencia optimista (dos pujas simultáneas → 409).
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Category Category { get; set; } = null!;
    public User Seller { get; set; } = null!;
    public User? HighestBidder { get; set; }
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
