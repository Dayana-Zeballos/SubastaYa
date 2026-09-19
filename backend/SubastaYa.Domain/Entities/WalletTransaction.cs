namespace SubastaYa.Domain.Entities;

public enum WalletTransactionType
{
    Deposit = 0,
    Reserve = 1,
    Release = 2,
    Capture = 3,
    Refund = 4,
    // Acreditación al vendedor cuando el worker liquida una venta.
    Payout = 5
}

public class WalletTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public Guid? AuctionId { get; set; }
    public WalletTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Wallet Wallet { get; set; } = null!;
}
