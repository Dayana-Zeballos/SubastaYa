namespace SubastaYa.Application.Features.Bidding;

public class BidHistoryProjection
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string BidderUserName { get; set; } = string.Empty;
}
