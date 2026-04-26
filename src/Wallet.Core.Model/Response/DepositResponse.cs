namespace Wallet.Core.Model.Response;

public class DepositResponse
{
    public Guid TransactionId { get; set; }
    public WalletBalanceResponse Wallet { get; set; } = null!;
    public bool IdempotentReplay { get; set; }
}
