namespace Wallet.Core.Model.Response;

public class WalletBalanceResponse
{
    public Guid WalletId { get; set; }
    public decimal MainBalance { get; set; }
    public decimal CashbackBalance { get; set; }
    public decimal TotalMainDeposited { get; set; }
    public decimal TotalMainWithdrawn { get; set; }
    public decimal TotalCashbackReceived { get; set; }
    public decimal TotalCashbackSpent { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
}
