namespace SubastaYa.Application.Features.Auctions;

public class AuctionDetailResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public decimal MinimumIncrement { get; set; }
    public decimal CurrentPrice { get; set; }

    // Monto exacto que la consola de puja sugiere por defecto. Mientras no haya ofertas es
    // el precio base, porque la primera puja puede igualarlo en lugar de superarlo.
    public decimal NextMinimumBid { get; set; }

    public int BidCount { get; set; }
    public string SellerUserName { get; set; } = string.Empty;

    // Seudónimo del postor líder. Nunca exponemos el nombre real de quien va ganando.
    public string? HighestBidderAlias { get; set; }

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public long SecondsRemaining { get; set; }

    // Solo tiene valor cuando la petición llega autenticada: le dice al frontend si tiene
    // que mostrar "Liderando" o "Superado" sin que el cliente compare identificadores.
    public bool? IsCurrentUserWinning { get; set; }
}
