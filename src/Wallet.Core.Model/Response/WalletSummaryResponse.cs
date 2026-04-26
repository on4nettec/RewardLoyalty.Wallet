namespace Wallet.Core.Model.Response;

public class WalletSummaryResponse
{
    public decimal MainBalance { get; set; }
    public decimal CashbackBalance { get; set; }
    public decimal SumCompletedCredits { get; set; }
    public decimal SumCompletedDebits { get; set; }
    public int CompletedTransactionCount { get; set; }
}
