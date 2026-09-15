namespace SubastaYa.Application.Features.Bidding;

public class BidHistoryItemResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    // Seudónimo del postor. Nunca devolvemos userId ni userName real.
    public string BidderAlias { get; set; } = string.Empty;
}
