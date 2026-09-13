namespace SubastaYa.Application.Features.Auctions;

// Lo que necesita una card del catálogo, ni más ni menos.
public class AuctionListItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public int BidCount { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Status { get; set; } = string.Empty;

    // Segundos que faltan para el cierre. El frontend arranca el contador con este número
    // en lugar de restar contra el reloj de la máquina del usuario, que puede estar corrido.
    public long SecondsRemaining { get; set; }
}
