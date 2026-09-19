namespace SubastaYa.Application.Common.Interfaces;

// La sala en vivo se entera por acá. Application no conoce SignalR:
// la API manda el evento al grupo de la subasta.
public interface IAuctionNotifier
{
    Task NotifyAsync(AuctionRealtimeEvent realtimeEvent, CancellationToken cancellationToken = default);
}

public class AuctionRealtimeEvent
{
    public string Event { get; set; } = string.Empty;
    public Guid AuctionId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? Status { get; set; }
}

public static class AuctionRealtimeEvents
{
    public const string BidPlaced = "bidPlaced";
    public const string AuctionExtended = "auctionExtended";
    public const string AuctionActivated = "auctionActivated";
    public const string AuctionClosed = "auctionClosed";
}
