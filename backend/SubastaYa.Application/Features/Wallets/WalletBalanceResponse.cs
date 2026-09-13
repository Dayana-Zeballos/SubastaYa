namespace SubastaYa.Application.Features.Wallets;

public class WalletBalanceResponse
{
    public Guid UserId { get; set; }
    public decimal Total { get; set; }
    public decimal Available { get; set; }
    public decimal Reserved { get; set; }
}
