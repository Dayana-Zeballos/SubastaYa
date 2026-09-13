namespace SubastaYa.Application.Features.Bidding;

public class BidResponse
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime AuctionEndsAt { get; set; }
    public bool AntiSnipingApplied { get; set; }
}
