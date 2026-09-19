namespace SubastaYa.Application.Features.Auctions;

// Los tres estados que muestra el catálogo. No son los mismos que AuctionStatus: acá
// "Finished" agrupa todo lo que ya cerró, sin importar cómo terminó.
public enum AuctionStatusFilter
{
    Active = 0,
    Scheduled = 1,
    Finished = 2
}

public enum AuctionSortOrder
{
    // Por defecto: primero las que están por cerrar, que es lo que genera urgencia.
    EndingSoon = 0,
    HighestBid = 1,
    LowestPrice = 2
}

public class AuctionQueryParameters
{
    public const int MaxPageSize = 50;

    public AuctionStatusFilter? Status { get; set; }
    public string? CategorySlug { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Search { get; set; }
    public AuctionSortOrder Sort { get; set; } = AuctionSortOrder.EndingSoon;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
